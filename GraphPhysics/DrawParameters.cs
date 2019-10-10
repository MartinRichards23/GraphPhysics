namespace GraphPhysics
{
    public class DrawParameters
    {
        readonly double zoomLevel;

        public DrawParameters(double zoomLevel)
        {
            this.zoomLevel = zoomLevel;
        }

        public double ZoomLevel
        {
            get { return zoomLevel; }
        }
    }
}
