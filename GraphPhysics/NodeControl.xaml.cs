using GraphPhysics.Model;
using GraphPhysics.Physics;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using SystemPlus.Windows;
using SystemPlus.Windows.Media;

namespace GraphPhysics
{
    /// <summary>
    /// Interaction logic for NodeControl.xaml
    /// </summary>
    public partial class NodeControl : INotifyPropertyChanged
    {
        #region Fields

        MouseMode mouseMode = MouseMode.None;
        Point mouseDownPoint;
        Point mousePoint;
        NodeBase clickedNode;

        readonly Pen newEdgePen;
        readonly Pen marqueePen;
        readonly Brush marqueeFill;

        public event PropertyChangedEventHandler PropertyChanged;
        public event GraphItemClickedHandler NodeClicked;
        EdgeAddedHandler edgeAddingHandler;

        SelectedItemCollection<NodeBase> selectedNodes = new SelectedItemCollection<NodeBase>();
        SelectedItemCollection<EdgeBase> selectedEdges = new SelectedItemCollection<EdgeBase>();

        const double maxZoom = 10;
        const double minZoom = 0.01;
        const double zoomAmount = 0.1;

        int frames = 0;
        DateTime fpsTime = DateTime.UtcNow;

        #endregion

        public NodeControl()
        {
            InitializeComponent();

            // set the default handler for adding edges
            edgeAddingHandler = delegate(NodeBase from, NodeBase to)
            {
                EdgeBase edge = new Edge(from, to);
                myCanvas.AddEdge(edge);
            };

            newEdgePen = new Pen(BrushCache.GetBrush(Colors.White), 3);
            newEdgePen.Freeze();
            marqueePen = new Pen(BrushCache.GetBrush(Colors.White), 3);
            marqueePen.Freeze();
            marqueeFill = BrushCache.GetBrush(Color.FromArgb(50, 255, 255, 255));
            
            ResetView();
        }

        #region Properties

        public PhysicsProvider Physics
        {
            get { return myCanvas.Physics; }
            set
            {
                if (myCanvas.Physics != value)
                {
                    myCanvas.Physics = value;
                    OnPropertyChanged("Physics");
                }
            }
        }

        public EdgeAddedHandler EdgeAddingHandler
        {
            get { return edgeAddingHandler; }
            set { edgeAddingHandler = value; }
        }

        #endregion

        #region Dependency properties

        public static readonly DependencyProperty CentreProperty = DependencyProperty.Register("Centre", typeof(Point), typeof(NodeControl), new PropertyMetadata(new Point(), OnCentreChanged, CoerceCentre));

        public static readonly DependencyProperty ZoomProperty = DependencyProperty.Register("Zoom", typeof(double), typeof(NodeControl), new PropertyMetadata(1.0, OnZoomChanged, CoerceZoom));

        static void OnCentreChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Point point = (Point)e.NewValue;
            NodeControl nodeControl = (NodeControl)d;

            nodeControl.myCanvas.Centre = point;
            nodeControl.myCanvas.UpdateViewPortRect();
        }

        static object CoerceCentre(DependencyObject d, object value)
        {
            Point point = (Point)value;
            NodeControl nodeControl = (NodeControl)d;
            return point;
        }

        public Point Centre
        {
            get { return (Point)GetValue(CentreProperty); }
            set { SetValue(CentreProperty, value); }
        }


        static void OnZoomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            double zoom = (double)e.NewValue;
            NodeControl nodeControl = (NodeControl)d;

            nodeControl.myCanvas.Zoom = zoom;
            nodeControl.myCanvas.UpdateViewPortRect();
        }

        static object CoerceZoom(DependencyObject d, object value)
        {
            double zoom = (double)value;
            NodeControl nodeControl = (NodeControl)d;

            zoom = Math.Max(minZoom, zoom);
            zoom = Math.Min(maxZoom, zoom);

            return zoom;
        }

        public double Zoom
        {
            get { return (double)GetValue(ZoomProperty); }
            set { SetValue(ZoomProperty, value); }
        }

        #endregion

        #region Event handlers

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        protected void OnNodeClicked(NodeBase node, MouseButtonEventArgs args)
        {
            NodeClicked?.Invoke(new GraphItemClickedEventArgs(node, args));
        }

        void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            //Point p1 = e.GetPosition(this);
            //Point p2 = myCanvas.ToWorldCoordinates(p1);

            Keyboard.Focus(this);

            mouseDownPoint = e.GetPosition(myCanvas);
            CaptureMouse();

            Quadrant q = myCanvas.QuadTree.GetQuadrant(mouseDownPoint);

            NodeBase mouseDownNode = myCanvas.NodeFromPoint(mouseDownPoint);
            EdgeBase mouseDownEdge = myCanvas.EdgeFromPoint(mouseDownPoint);

            clickedNode = mouseDownNode;

            if (e.ChangedButton == MouseButton.Left)
            {
                if (mouseDownNode != null)
                {
                    if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                    {
                        mouseMode = MouseMode.AddingEdge;
                    }
                    else
                    {
                        mouseMode = MouseMode.DraggingNode;

                        OnNodeClicked(clickedNode, e);

                        //if (e.ClickCount == 1)
                        //{

                        //}
                        //else if (e.ClickCount == 2)
                        //{
                        //    for (int i = 0; i < 10; i++)
                        //    {
                        //        Point p = new Point(clickedNode.Position.X + 10 + (5 * i), clickedNode.Position.Y);

                        //        NodeBase n = new Node(p, 1, clickedNode.Colour);
                        //        EdgeBase edge = new Edge(clickedNode, n);

                        //        myCanvas.AddNode(n);
                        //        myCanvas.AddEdge(edge);
                        //    }
                        //}
                    }
                }
                else if( mouseDownEdge != null)
                {

                }
                else
                {
                    selectedNodes.Clear();
                    selectedEdges.Clear();

                    clickedNode = null;

                    if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                    {
                        mouseMode = MouseMode.MarqueeSelecting;
                    }
                    else
                    {
                        mouseMode = MouseMode.Panning;
                    }
                }
            }
        }

        void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            e.Handled = true;

            Point newPoint = e.GetPosition(myCanvas);
            Vector mouseDelta = newPoint - mousePoint;
            Vector mouseTotalDelta = newPoint - mouseDownPoint;

            mousePoint = newPoint;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (mouseMode == MouseMode.DraggingNode)
                {
                    foreach (NodeBase node in selectedNodes)
                    {
                        node.Position += mouseDelta;
                    }
                }
                else if (mouseMode == MouseMode.AddingEdge)
                {
                }
                else if (mouseMode == MouseMode.Panning)
                {
                    // todo: use mouseDelta, not totalDelta
                    Centre -= mouseTotalDelta;
                }
            }

            txtMousePos.Text = string.Format("Mouse: {0:f1}", mousePoint);
            txtCentrePos.Text = string.Format("Centre: {0:f1}", myCanvas.Centre);
            txtTpoLeftPos.Text = string.Format("Top left: {0:f1}", myCanvas.Position);
            txtContent.Text = string.Format("Content top left: {0:f1}", myCanvas.GetContentBounds().TopLeft);
        }

        void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            ReleaseMouseCapture();
            Point upPoint = e.GetPosition(myCanvas);

            if (mouseMode == MouseMode.DraggingNode)
            {
                if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    selectedNodes.Clear();
                    selectedEdges.Clear();
                }

                if (clickedNode != null)
                {
                    //OnNodeClicked(clickedNode, e);
                    selectedNodes.Add(clickedNode);
                }
            }
            else if (mouseMode == MouseMode.AddingEdge && clickedNode != null)
            {
                NodeBase upNode = myCanvas.NodeFromPoint(upPoint);

                if (upNode != null)
                {
                    edgeAddingHandler?.Invoke(clickedNode, upNode);
                }

                // todo: set momentum of node
                // myCanvas.SelectedNode.Point = mousePoint;
                // myCanvas.SelectedNode.Momentum = mouseDelta;               
            }
            else if (mouseMode == MouseMode.MarqueeSelecting)
            {
                Point a = mouseDownPoint;
                Point b = mousePoint;
                Rect marquee = new Rect(a, b);

                NodeBase[] pickedNodes = myCanvas.QuadTree.GetNodesInRect(marquee).ToArray();
                selectedNodes.AddRange(pickedNodes);
            }

            mouseMode = MouseMode.None;
        }

        void UserControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point mousePoint = e.GetPosition(myCanvas);

            if (e.Delta > 0)
                ZoomIn(mousePoint);
            else
                ZoomOut(mousePoint);
        }

        private void UserControl_PreviewMouseMove(object sender, MouseEventArgs e)
        {

        }

        private void UserControl_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void UserControl_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void UserControl_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {

        }

        void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            myCanvas.UpdateViewPortRect();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.OemPlus)
            {
                ZoomIn();
            }
            else if (e.Key == Key.OemMinus)
            {
                ZoomOut();
            }
            else if (e.Key == Key.Up || e.Key == Key.NumPad8)
            {
                PanUp();
            }
            else if (e.Key == Key.Down || e.Key == Key.NumPad8)
            {
                PanDown();
            }
            else if (e.Key == Key.Left || e.Key == Key.NumPad8)
            {
                PanLeft();
            }
            else if (e.Key == Key.Right || e.Key == Key.NumPad8)
            {
                PanRight();
            }
            else if (e.Key == Key.Home)
            {
                ResetView();
            }
            else if (e.Key == Key.End)
            {
                FitAll();
            }

            e.Handled = true;

            base.OnPreviewKeyDown(e);
        }

        private void myCanvas_Rendering(DrawingContext dc)
        {
            frames++;
            DateTime now = DateTime.UtcNow;
            if ((now - fpsTime).TotalSeconds >= 1)
            {
                fpsTime = now;
                txtFps.Text = string.Format("FPS:{0}, PPS:{1}", frames, Physics.CurrentFPS);

                frames = 0;
            }

            if (mouseMode == MouseMode.AddingEdge)
            {
                if (clickedNode != null)
                {
                    dc.DrawLine(newEdgePen, clickedNode.Position, mousePoint);
                }
            }
            else if (mouseMode == MouseMode.MarqueeSelecting)
            {
                Rect rect = new Rect(mouseDownPoint, mousePoint);
                dc.DrawRectangle(marqueeFill, marqueePen, rect);
            }
        }

        #endregion

        public void AddNode(NodeBase node)
        {
            myCanvas.AddNode(node);
        }

        public void AddEdge(EdgeBase edge)
        {
            myCanvas.AddEdge(edge);
        }

        public void ClearAll()
        {
            myCanvas.ClearAll();
        }


        #region Moving / zooming

        /// <summary>
        /// Animate moves current position
        /// </summary>
        public void AnimatePan(Point point)
        {
            PointAnimation animator = new PointAnimation();
            animator.From = Centre;
            animator.To = point;
            animator.DecelerationRatio = 0.5;
            animator.Duration = TimeSpan.FromSeconds(0.3);
            animator.FillBehavior = FillBehavior.Stop;

            animator.Completed += delegate
            {
                Centre = point;
                // OnFinishedMoving();
            };

            BeginAnimation(CentreProperty, animator);
        }

        public void AnimatePan(Vector vector)
        {
            AnimatePan(Centre + vector);
        }

        /// <summary>
        /// Animate moves current position
        /// </summary>
        public void AnimateZoom(double zoom)
        {
            DoubleAnimation animator = new DoubleAnimation();
            animator.From = Zoom;
            animator.To = zoom;
            animator.DecelerationRatio = 0.5;
            animator.Duration = TimeSpan.FromSeconds(0.3);
            animator.FillBehavior = FillBehavior.Stop;

            animator.Completed += delegate
            {
                Zoom = zoom;
                // OnFinishedMoving();
            };

            BeginAnimation(ZoomProperty, animator);
        }

        /// <summary>
        /// Moves current position
        /// </summary>
        public void Pan(Vector vector)
        {
            Centre += vector;
        }

        /// <summary>
        /// Moves current position
        /// </summary>
        public void Pan(Point point)
        {
            Centre = point;
        }

        public void PanUp()
        {
            double amount = myCanvas.ViewPort.Height * 0.05;
            Pan(new Vector(0, -amount));
        }

        public void PanDown()
        {
            double amount = myCanvas.ViewPort.Height * 0.05;
            Pan(new Vector(0, amount));
        }

        public void PanLeft()
        {
            double amount = myCanvas.ViewPort.Height * 0.05;
            Pan(new Vector(-amount, 0));
        }

        public void PanRight()
        {
            double amount = myCanvas.ViewPort.Height * 0.05;
            Pan(new Vector(amount, 0));
        }

        /// <summary>
        /// Zooms and pans to fit all content
        /// </summary>
        public void FitAll()
        {
            Rect contentRect = myCanvas.GetContentBounds();
            Point contentCentre = contentRect.Center();

            double scaleX = myCanvas.ViewPort.Width / contentRect.Width;
            double scaleY = myCanvas.ViewPort.Height / contentRect.Height;
            double newZoom = Zoom * Math.Min(scaleX, scaleY);
            newZoom *= 0.95;

            AnimatePan(contentCentre);
            AnimateZoom(newZoom);
        }

        public void ResetView()
        {
            Zoom = 1;
            Centre = myCanvas.QuadTree.GetCentre();
        }

        public void ZoomIn()
        {
            ZoomIn(new Point(ActualWidth / 2, ActualHeight / 2));
        }

        public void ZoomOut()
        {
            ZoomOut(new Point(ActualWidth / 2, ActualHeight / 2));
        }

        public void ZoomIn(Point focus)
        {
            focus = myCanvas.ToViewCoordinates(focus);

            Zoom *= 1 + zoomAmount;

            //scale.CenterX = focus.X;
            //scale.CenterY = focus.Y;
        }

        public void ZoomOut(Point focus)
        {
            focus = myCanvas.ToViewCoordinates(focus);

            Zoom *= 1 - zoomAmount;

            // scale.CenterX = focus.X;
            //scale.CenterY = focus.Y;
        }

        #endregion
    }
}