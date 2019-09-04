using System;

namespace GraphPhysics.Physics
{
    internal class NoPhysics : PhysicsProvider
    {
        #region Fields

        #endregion

        #region Properties

        public override string Name
        {
            get { return "None"; }
        }

        #endregion

        protected override void UpdatePhysics(TimeSpan duration)
        {
        }
    }
}