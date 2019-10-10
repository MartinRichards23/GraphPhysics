using GraphPhysics.Model;
using GraphPhysics.Physics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using SystemPlus.Threading;
using SystemPlus.Windows;
using SystemPlus.Windows.Media;

namespace GraphPhysics
{
    public class MyCanvas : Canvas
    {
        #region Fields

        Rect viewPort;

        readonly Brush whiteBrush;
        readonly Brush fontColour;

        readonly ScaleTransform scale = new ScaleTransform();
        readonly TranslateTransform translate = new TranslateTransform();

        PhysicsProvider physics;
        readonly QuadTree quadTree;
        readonly List<EdgeBase> edges = new List<EdgeBase>();

        public event Action<DrawingContext> Rendering;

        DateTime lastDraw = DateTime.UtcNow;

        #endregion

        public MyCanvas()
        {
            IsHitTestVisible = false;

            TransformGroup transforms = new TransformGroup();
            transforms.Children.Add(translate);
            transforms.Children.Add(scale);

            //RenderTransformOrigin = new Point(0.5, 0.5);
            RenderTransform = transforms;

            quadTree = new QuadTree();
            Physics = new ForceDirectedPhysics();

            whiteBrush = BrushCache.GetBrush(Colors.White);
            fontColour = BrushCache.GetBrush(Colors.White);

            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Render);
            timer.Interval = TimeSpan.FromMilliseconds(16);
            timer.Tick += timer_Tick;
            timer.Start();

            Thread physicsThread = new Thread(RunPhysics);
            physicsThread.IsBackground = true;
            //physicsThread.Start();

            SizeChanged += MyCanvas_SizeChanged;
        }

        #region Properties

        /// <summary>
        /// Current zoom level
        /// </summary>
        public double Zoom
        {
            get { return scale.ScaleX; }
            set
            {
                scale.ScaleX = value;
                scale.ScaleY = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Current top left of the viewport
        /// </summary>
        public Point Position
        {
            get { return new Point(translate.X, translate.Y); }
            set
            {
                translate.X = value.X;
                translate.Y = value.Y;
                Invalidate();
            }
        }

        /// <summary>
        /// Current centre of the viewport
        /// </summary>
        public Point Centre
        {
            get { return new Point((this.ActualWidth / 2) - translate.X, (this.ActualHeight / 2) - translate.Y); }
            set
            {
                translate.X = (this.ActualWidth / 2) - value.X;
                translate.Y = (this.ActualHeight / 2) - value.Y;
                Invalidate();
            }
        }

        public Rect ViewPort
        {
            get { return viewPort; }
        }

        internal List<NodeBase> Nodes
        {
            get { return new List<NodeBase>(); }
        }

        internal List<EdgeBase> Edges
        {
            get { return edges; }
        }

        internal PhysicsProvider Physics
        {
            get { return physics; }
            set
            {
                physics = value;
                if (physics != null)
                    physics.Initialise(this);
            }
        }

        internal QuadTree QuadTree
        {
            get { return quadTree; }
        }

        #endregion

        #region Adding / removing

        public void AddNode(NodeBase node)
        {
            quadTree.AddNode(node);
            Invalidate();
        }

        public void AddEdge(EdgeBase edge)
        {
            edges.Add(edge);
            Invalidate();
        }

        public void ClearAll()
        {
            quadTree.Clear();
            edges.Clear();
            Invalidate();
        }

        #endregion

        #region Zooming / Panning

        /// <summary>
        /// Gets the smallest rect that contains all the nodes
        /// </summary>
        public Rect GetContentBounds()
        {
            Rect contentBounds = quadTree.AllNodes().Select(n => n.GetBounds()).GetBounds();
            return contentBounds;
        }

        #endregion

        /// <summary>
        /// Causes the canvas to render
        /// </summary>
        public void Invalidate()
        {
            InvalidateVisual();
        }

        void RunPhysics()
        {
            DateTime lastUpdate = DateTime.UtcNow;
            Delayer delayer = new Delayer(TimeSpan.FromMilliseconds(25));

            while (true)
            {
                delayer.Delay();

                DateTime now = DateTime.UtcNow;
                TimeSpan duration = now - lastUpdate;
                lastUpdate = now;

                physics.DoPhysics(duration);
            }
        }

        void MyCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            scale.CenterX = this.ActualWidth / 2;
            scale.CenterY = this.ActualHeight / 2;
            Invalidate();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            Invalidate();
        }

        protected override void OnRender(DrawingContext dc)
        {
#if DEBUG
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
                // Design-mode specific functionality
            }
#endif

            DrawParameters parameters = new DrawParameters(scale.ScaleX);

            DateTime now = DateTime.UtcNow;
            TimeSpan duration = now - lastDraw;
            lastDraw = now;

            // update physics
            physics.DoPhysics(duration);

            if (Rendering != null)
                Rendering(dc);

            Quadrant topLeftQuad = quadTree.GetQuadrant(viewPort.TopLeft);
            Quadrant bottomRightQuad = quadTree.GetQuadrant(viewPort.BottomRight);

            // draw edges first
            for (int x = topLeftQuad.X; x <= bottomRightQuad.X; x++)
            {
                for (int y = topLeftQuad.Y; y <= bottomRightQuad.Y; y++)
                {
                    Quadrant q = quadTree.Quadrants[x, y];

                    // draw quadrant
                    //dc.DrawRectangle(null, new Pen(whiteBrush, 1), q.Rect);
                    //dc.DrawText(q.Rect.TopLeft.ToString(), 12, whiteBrush, q.Rect.TopLeft);

                    // draw all the nodes in this quadrant
                    foreach (NodeBase n in q.Nodes)
                    {
                        foreach (EdgeBase e in n.FromEdges)
                        {
                            e.Draw(dc, parameters);
                        }
                    }
                }
            }

            // draw nodes
            for (int x = topLeftQuad.X; x <= bottomRightQuad.X; x++)
            {
                for (int y = topLeftQuad.Y; y <= bottomRightQuad.Y; y++)
                {
                    Quadrant q = quadTree.Quadrants[x, y];

                    // draw all the nodes in this quadrant
                    foreach (NodeBase n in q.Nodes)
                    {
                        n.Draw(dc, parameters);
                    }
                }
            }

            // draw content bounds
            //if (quadTree.AllNodes().Any())
            //    dc.DrawRectangle(null, new Pen(whiteBrush, 1), GetContentBounds());

            // draw viewport
            //dc.DrawRectangle(null, new Pen(whiteBrush, 2), viewPort);
        }

        public Point ToViewCoordinates(Point point)
        {
            return RenderTransform.Transform(point);
        }

        public Point ToWorldCoordinates(Point point)
        {
            return RenderTransform.Inverse.Transform(point);
        }

        public void UpdateViewPortRect()
        {
            // calc the view port window
            viewPort = new Rect(0, 0, ActualWidth, ActualHeight);
            viewPort = RenderTransform.TransformBounds(viewPort);

            Point topLeft = RenderTransform.Inverse.Transform(new Point(0, 0));
            Point bottomRight = RenderTransform.Inverse.Transform(new Point(ActualWidth, ActualHeight));

            Rect r = new Rect(topLeft, bottomRight);

            viewPort = r;

        }

        public NodeBase NodeFromPoint(Point p)
        {
            Quadrant q = quadTree.GetQuadrant(p);

            foreach (NodeBase n in q.CloseNodes())
            {
                if (n.HitTest(p))
                    return n;
            }

            return null;
        }

        public EdgeBase EdgeFromPoint(Point p)
        {
            foreach (EdgeBase e in edges)
            {
                if (e.HitTest(p))
                    return e;
            }

            return null;
        }
    }
}