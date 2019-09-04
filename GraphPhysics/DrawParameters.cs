using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
