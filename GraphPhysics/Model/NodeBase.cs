using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using SystemPlus.Windows;

namespace GraphPhysics.Model
{
    public abstract class NodeBase : GraphItem
    {
        #region Fields

        static int indexCount = 0;

        Point position;
        Vector velocity;

        internal int index;
        internal Quadrant CurrentQuadrant;

        internal Vector TotalForce;

        double mass = 1;
        double charge = 1;

        bool isPositionLocked;

        readonly List<EdgeBase> fromEdges = new List<EdgeBase>();
        readonly List<EdgeBase> toEdges = new List<EdgeBase>();

        #endregion

        protected NodeBase(Point p, double mass, Color colour)
            : base(colour)
        {
            index = indexCount++;
            Position = p;
            Mass = mass;        
        }

        #region Properties

        public Point Position
        {
            get { return position; }
            set { position = value; }
        }

        public Vector Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }

        public double Charge
        {
            get { return charge; }
            set { charge = value; }
        }

        public double Mass
        {
            get { return mass; }
            set { mass = value; }
        }

        public bool IsPositionLocked
        {
            get { return isPositionLocked; }
            set { isPositionLocked = value; }
        }

        public double Radius
        {
            get { return 25 + mass * 10; }
        }

        /// <summary>
        /// Edges from this node
        /// </summary>
        internal IList<EdgeBase> FromEdges
        {
            get { return fromEdges; }
        }

        /// <summary>
        /// Edges to this node
        /// </summary>
        internal IList<EdgeBase> ToEdges
        {
            get { return toEdges; }
        }

        #endregion

        internal void AddEdge(EdgeBase e)
        {
            if (e.From == this)
                fromEdges.Add(e);
            else
                toEdges.Add(e);

            //mass += 0.01;
            //charge += 0.01;
        }

        public virtual Rect GetBounds()
        {
            double r = Radius;

            return new Rect(position.X - r, position.Y - r, r * 2, r * 2);
        }

        public override bool HitTest(Point p)
        {
            double radius = Radius + 2;
            double dist = VectorTools.Distance(position, p);
            if (dist < radius)
                return true;

            return false;
        }       
    }
}