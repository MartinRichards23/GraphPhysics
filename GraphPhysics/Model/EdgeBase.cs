using System.Windows;
using System.Windows.Media;
using SystemPlus.Windows;

namespace GraphPhysics.Model
{
    public abstract class EdgeBase : GraphItem
    {
        #region Fields

        readonly NodeBase from;
        readonly NodeBase to;

        double thickness = 2;

        #endregion

        protected EdgeBase(NodeBase from, NodeBase to, Color colour)
            : base(colour)
        {
            this.from = from;
            this.to = to;

            from.AddEdge(this);
            to.AddEdge(this);
        }

        #region Properties

        public NodeBase From
        {
            get { return from; }
        }

        public NodeBase To
        {
            get { return to; }
        }

        public double Thickness
        {
            get { return thickness; }
        }

        #endregion

        public override bool HitTest(Point p)
        {
            double dist = VectorTools.DistanceToLine(from.Position, to.Position, p);

            if (dist < thickness + 2)
                return true;

            return false;
        }

    }
}