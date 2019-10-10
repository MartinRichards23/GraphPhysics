using GraphPhysics.Model;
using GraphPhysics.Physics;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SystemPlus.Windows;
using SystemPlus.Windows.Media;

namespace GraphPhysics
{
    public partial class MainWindow
    {
        readonly Random rand = new Random();

        public MainWindow()
        {
            InitializeComponent();

            cmboPhysics.Items.Add(new NoPhysics());
            cmboPhysics.Items.Add(new ForceDirectedPhysics());
            cmboPhysics.Items.Add(new NewtonianPhysics());

            cmboPhysics.SelectedIndex = 1;
        }

        #region Event handlers

        void CmboPhysics_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PhysicsProvider physics = (PhysicsProvider)cmboPhysics.SelectedItem;
            myNodeControl.Physics = physics;
        }

        void BtnAddGrid_Click(object sender, RoutedEventArgs e)
        {
            AddGrid(10, 10, chkTangled.IsChecked == true, chkRandomMass.IsChecked == true);
        }

        void BtnAddTree_Click(object sender, RoutedEventArgs e)
        {
            AddTree(4);
        }

        private void BtnSpiral_Click(object sender, RoutedEventArgs e)
        {
            AddSpiral(100);
        }

        void MyNodeControl_NodeClicked(GraphItemClickedEventArgs args)
        {
            NodeBase node = args.Item as NodeBase;

            if (args.MouseButtonArgs.ClickCount == 1)
            {
                panelNodeOptions.DataContext = node;
            }
            else if (args.MouseButtonArgs.ClickCount == 3 && node != null)
            {
                myNodeControl.AnimatePan(node.Position);
                myNodeControl.AnimateZoom(1);
            }
        }

        void Button_Click(object sender, RoutedEventArgs e)
        {
            myNodeControl.ResetView();
        }

        void BtnClearData_Click(object sender, RoutedEventArgs e)
        {
            myNodeControl.ClearAll();
        }

        private void BtnFitAll_Click(object sender, RoutedEventArgs e)
        {
            myNodeControl.FitAll();
        }

        #endregion

        public void AddOne()
        {
            NodeBase n1 = new Node(myNodeControl.Centre, 1, Colors.Red);
            NodeBase n2 = new Node(myNodeControl.Centre + new Vector(100, 100), 1, Colors.Blue);

            myNodeControl.AddNode(n1);
            myNodeControl.AddNode(n2);
            myNodeControl.AddEdge(new Edge(n1, n2));
        }

        public void AddGrid(int width, int height, bool tangled, bool randomMass)
        {
            Point origin = myNodeControl.Centre;

            int total = width * height;
            NodeBase[,] grid = new NodeBase[width, height];
            int count = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Color col = ColourTools.GetRainbowColor(count, 0, total);
                    double mass = 0.5;
                    if (randomMass)
                        mass = 0.5 + rand.NextDouble() * 2;

                    Point p;
                    if (tangled)
                        p = rand.NextPoint(origin, 400);
                    else
                        p = new Point(origin.X + x * 60, origin.Y + y * 60);

                    NodeBase n = new Node(p, mass, col);
                    grid[x, y] = n;
                    myNodeControl.AddNode(n);
                    count++;

                    if (x > 0)
                    {
                        NodeBase other = grid[x - 1, y];

                        EdgeBase edge = new Edge(n, other);
                        myNodeControl.AddEdge(edge);
                    }

                    if (y > 0)
                    {
                        NodeBase other = grid[x, y - 1];

                        EdgeBase edge = new Edge(n, other);
                        myNodeControl.AddEdge(edge);
                    }
                }
            }
        }

        public void AddTree(int maxDepth)
        {
            Point origin = myNodeControl.Centre;

            Color col = ColourTools.GetRainbowColor(0, 0, maxDepth);
            double mass = 2;
            NodeBase node = new Node(origin, mass, col);
            myNodeControl.AddNode(node);

            AddSubNodes(node, 0, maxDepth);
        }

        void AddSubNodes(NodeBase node, int depth, int maxDepth)
        {
            if (depth >= maxDepth)
                return;

            int children = rand.Next(1, maxDepth - depth + 3);

            double size = Math.Max(0.2, rand.NextDouble() * children / 4);
            node.Mass += size;
            node.Charge += size;

            for (int i = 0; i < children; i++)
            {
                Color col = ColourTools.GetRainbowColor(depth, 0, maxDepth);
                double mass = 0.5;
                Point p = rand.NextPoint(node.Position, 50);

                NodeBase child = new Node(p, mass, col);
                myNodeControl.AddNode(child);

                EdgeBase edge = new Edge(node, child);
                myNodeControl.AddEdge(edge);

                AddSubNodes(child, depth + 1, maxDepth);
            }
        }

        public void AddSpiral(int count)
        {
            Point origin = myNodeControl.Centre;
            SpiralMaker spiral = new SpiralMaker(origin, 5, 200, 200);

            NodeBase parent = new Node(origin, 0.5, Colors.Green);
            myNodeControl.AddNode(parent);

            for (int i = 0; i < count; i++)
            {
                Color col = ColourTools.GetRainbowColor(i, 0, count);
                double mass = 0.5;

                Point p = spiral.NextPoint();

                NodeBase child = new Node(p, mass, col);
                myNodeControl.AddNode(child);

                EdgeBase edge = new Edge(parent, child);
                myNodeControl.AddEdge(edge);
            }
        }

    }
}