using System;
using System.Windows;
using GraphPhysics.Model;

namespace GraphPhysics.Physics
{
    internal class NewtonianPhysics : PhysicsProvider
    {
        #region Fields

        double gravityStrength = 1;

        #endregion

        public NewtonianPhysics()
        {
            Friction = 0.04;
            BounceNodes = true;
        }

        #region Properties

        public override string Name
        {
            get { return "Newtonian"; }
        }

        public double GravityStrength
        {
            get { return gravityStrength; }
            set
            {
                if (gravityStrength != value)
                {
                    gravityStrength = value;
                    OnPropertyChanged("GravityStrength");
                }
            }
        }

        #endregion

        protected override void UpdatePhysics(TimeSpan duration)
        {
            double floor = canvas.ViewPort.Y + canvas.ViewPort.Height;

            foreach (NodeBase n in quadTree.AllNodes())
            {
                if (n.IsSelected)
                    continue;

                Vector totalForce = new Vector();

                if (n.Position.Y + n.Radius >= floor)
                {
                    // has hit the floor
                    if (n.Velocity.Y > 0)
                    {
                        // invert velocity
                        n.Velocity = new Vector(n.Velocity.X, n.Velocity.Y * -bounceEfficiency);
                    }
                }
                else
                {
                    totalForce = new Vector(0, gravityStrength * n.Mass);
                }

                n.TotalForce = totalForce;
            }
        }
    }
}