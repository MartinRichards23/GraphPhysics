using GraphPhysics.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using SystemPlus;
using SystemPlus.ComponentModel;
using SystemPlus.Windows;

namespace GraphPhysics.Physics
{
    public abstract class PhysicsProvider : NotifyPropertyChanged
    {
        #region Fields

        protected MyCanvas canvas;
        protected List<NodeBase> nodes;
        protected List<EdgeBase> edges;
        protected QuadTree quadTree;

        protected double friction = 0.05;
        private double time = 1;

        protected double bounceEfficiency = 0.8;

        bool bounceNodes;

        double frames = 0;
        double currentFPS = 0;
        DateTime fpsTime = DateTime.UtcNow;

        #endregion

        internal void Initialise(MyCanvas canvas)
        {
            this.canvas = canvas;
            this.quadTree = canvas.QuadTree;
            nodes = canvas.Nodes;
            edges = canvas.Edges;
        }

        #region Properties

        public double Friction
        {
            get { return friction; }
            set
            {
                if (friction != value)
                {
                    friction = value;
                    OnPropertyChanged("Friction");
                }
            }
        }

        public double Time
        {
            get { return time; }
            set 
            {
                if (time != value)
                {
                    time = value;
                    OnPropertyChanged("Time");
                }
            }
        }

        public double BounceEfficiency
        {
            get { return bounceEfficiency; }
            set
            {
                if (bounceEfficiency != value)
                {
                    bounceEfficiency = value;
                    OnPropertyChanged("BounceEfficiency");
                }
            }
        }

        public bool BounceNodes
        {
            get { return bounceNodes; }
            set
            {
                if (bounceNodes != value)
                {
                    bounceNodes = value;
                    OnPropertyChanged("BounceNodes");
                }
            }
        }

        public double CurrentFPS
        {
            get { return currentFPS; }
        }

        public abstract string Name { get; }

        #endregion

        public void DoPhysics(TimeSpan duration)
        {
            frames++;
            DateTime now = DateTime.UtcNow;
            if ((now - fpsTime).TotalSeconds >= 1)
            {
                fpsTime = now;
                currentFPS = frames;
                frames = 0;
            }

            UpdatePhysics(duration);

            if (bounceNodes)
                UpdateBounces(duration);

            UpdatePositions(duration);
        }

        protected abstract void UpdatePhysics(TimeSpan duration);

        protected virtual void UpdatePositions(TimeSpan duration)
        {
            double timeFactor = (duration.TotalMilliseconds / 25) * time;
            time = MathTools.Clip(time, 0.1, 10);

            foreach (Node node in quadTree.AllNodes())
            {
                if (node.IsSelected || node.IsPositionLocked)
                {
                    // reset force
                    node.TotalForce = new Vector();
                    node.Velocity = new Vector();
                }

                // add drag, todo: should be proportional to V, plus a fixed amount
                node.TotalForce -= (node.Velocity * friction);
                //if (node.Velocity.Length > 0)
                //{
                //    double dragStrength = 0.2 + (node.Velocity.LengthSquared / 100) * friction;

                //    Vector dragForce = node.Velocity;
                //    dragForce.Normalize();
                //    dragForce *= dragStrength;

                //    node.TotalForce -= dragForce;
                //}

                // calculate the acceleration, F = MA
                Vector acceleration = node.TotalForce / node.Mass;

                // calculate the new velocity, todo: should be V = AT
                node.Velocity += acceleration * timeFactor;

                // calculate the new position, V = D/T
                node.Position += node.Velocity * timeFactor;

                // reset force
                node.TotalForce = new Vector();

                quadTree.UpdateNodeQuadrant(node);
            }
        }

        protected virtual void UpdateBounces(TimeSpan duration)
        {
            Parallel.ForEach(quadTree.AllQuadrants(), quad =>
            {
                for (int i = 0; i < quad.Nodes.Count; i++)
                {
                    NodeBase nodeA = quad.Nodes[i];

                    // find bounces
                    for (int j = i + 1; j < quad.Nodes.Count; j++)
                    {
                        NodeBase nodeB = quad.Nodes[j];

                        double dist = VectorTools.Distance(nodeA.Position, nodeB.Position);
                        if (dist <= nodeA.Radius + nodeB.Radius)
                        {
                            CalcNodeCollision(nodeA, nodeB, bounceEfficiency);
                        }
                    }
                }
            });
        }

        static void CalcNodeCollision(NodeBase nodeA, NodeBase nodeB, double bounceEfficiency)
        {
            double length = (nodeA.Position - nodeB.Position).Length;

            double ex = (nodeA.Position.X - nodeB.Position.X) / length;
            double ey = (nodeA.Position.Y - nodeB.Position.Y) / length;

            double ox = ey * -1;
            double oy = ex;

            double e1x = (nodeA.Velocity.X * ex + nodeA.Velocity.Y * ey) * ex;
            double e1y = (nodeA.Velocity.X * ex + nodeA.Velocity.Y * ey) * ey;
            double e2x = (nodeB.Velocity.X * ex + nodeB.Velocity.Y * ey) * ex;
            double e2y = (nodeB.Velocity.X * ex + nodeB.Velocity.Y * ey) * ey;

            double o1x = (nodeA.Velocity.X * ox + nodeA.Velocity.Y * oy) * ox;
            double o1y = (nodeA.Velocity.X * ox + nodeA.Velocity.Y * oy) * oy;
            double o2x = (nodeB.Velocity.X * ox + nodeB.Velocity.Y * oy) * ox;
            double o2y = (nodeB.Velocity.X * ox + nodeB.Velocity.Y * oy) * oy;
            double vxs = (nodeA.Mass * e1x + nodeB.Mass * e2x) / (nodeA.Mass + nodeB.Mass);
            double vys = (nodeA.Mass * e1y + nodeB.Mass * e2y) / (nodeA.Mass + nodeB.Mass);

            //Velocity nodeA after collision
            double vx1 = (-e1x + 2 * vxs + o1x) * bounceEfficiency;
            double vy1 = (-e1y + 2 * vys + o1y) * bounceEfficiency;

            //Velocity nodeB after collision
            double vx2 = (-e2x + 2 * vxs + o2x) * bounceEfficiency;
            double vy2 = (-e2y + 2 * vys + o2y) * bounceEfficiency;

            nodeA.Velocity = new Vector(vx1, vy1);
            nodeB.Velocity = new Vector(vx2, vy2);
        }

        public override string ToString()
        {
            return Name;
        }

        public static bool IsBad(Vector v)
        {
            if (double.IsNaN(v.X) || double.IsInfinity(v.X) || double.IsNegativeInfinity(v.X) || double.IsPositiveInfinity(v.X))
            {
                return true;
            }
            if (double.IsNaN(v.Y) || double.IsInfinity(v.Y) || double.IsNegativeInfinity(v.Y) || double.IsPositiveInfinity(v.Y))
            {
                return true;
            }

            return false;
        }
    }
}