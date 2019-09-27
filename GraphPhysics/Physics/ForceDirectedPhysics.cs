using GraphPhysics.Model;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace GraphPhysics.Physics
{
    /// <summary>
    /// Force directed Node-Edge physics implementation
    /// </summary>
    internal class ForceDirectedPhysics : PhysicsProvider
    {
        #region Fields

        double charge = 6;
        double edgeSpring = 0.003;

        #endregion

        #region Properties

        public override string Name
        {
            get { return "Force directed"; }
        }

        public double Charge
        {
            get { return charge; }
            set
            {
                if (charge != value)
                {
                    charge = value;
                    OnPropertyChanged("Charge");
                }
            }
        }

        public double EdgeSpring
        {
            get { return edgeSpring; }
            set
            {
                if (edgeSpring != value)
                {
                    edgeSpring = value;
                    OnPropertyChanged("EdgeSpring");
                }
            }
        }

        #endregion

        private void AddRepulsionForce(NodeBase nodeA, NodeBase nodeB)
        {
            Vector v = nodeA.Position - nodeB.Position;
            double dist = v.Length - nodeA.Radius - nodeB.Radius;
            dist = Math.Max(dist, 1);

            double strength = (Math.Sqrt(charge / dist)) * nodeA.Charge * nodeB.Charge;
            v.Normalize();

            if (strength > 0.1)
            {
                Vector v2 = strength * v;
                nodeA.TotalForce += v2;
                nodeB.TotalForce -= v2;
            }
        }

        private void AddAttractionForce(EdgeBase edge)
        {
            NodeBase nodeFrom = edge.From;
            NodeBase nodeTo = edge.To;

            Vector v = nodeFrom.Position - nodeTo.Position;
            double dist = v.Length - nodeFrom.Radius - nodeTo.Radius;
            dist = Math.Max(dist, 1);

            double strength = (Math.Pow(edgeSpring * dist, 1.8)) * nodeFrom.Mass * nodeTo.Mass;

            // clip max strength
            //strength = Math.Min(strength, 200);

            Vector normal = v;
            normal.Normalize();

            if (strength > 0.1)
            {
                Vector v2 = strength * normal;
                nodeFrom.TotalForce -= v2;
                nodeTo.TotalForce += v2;
            }
        }

        protected override void UpdatePhysics(TimeSpan duration)
        {
            Parallel.ForEach(quadTree.AllQuadrants(), quad =>
            //foreach(Quadrant quad in quadTree.AllQuadrants())
            {
                // compare against all other nodes in same quadrant
                for (int x = 0; x < quad.Nodes.Count; x++)
                {
                    NodeBase nodeA = quad.Nodes[x];

                    for (int y = x + 1; y < quad.Nodes.Count; y++)
                    {
                        NodeBase nodeB = quad.Nodes[y];
                        AddRepulsionForce(nodeA, nodeB);
                    }
                }

                // compare against all nodes in surrounding quadrants
                foreach (NodeBase nodeA in quad.Nodes)
                {
                    foreach (NodeBase nodeB in quad.AdjancentNodes())
                    {
                        AddRepulsionForce(nodeA, nodeB);
                    }

                    // calc edge forces from this node
                    for (int j = 0; j < nodeA.FromEdges.Count; j++)
                    {
                        EdgeBase edge = nodeA.FromEdges[j];
                        AddAttractionForce(edge);                        
                    }
                }
            });
        }
    }
}