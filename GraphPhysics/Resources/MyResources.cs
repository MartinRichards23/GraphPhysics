using System.IO;
using System.Windows.Media.Imaging;
using SystemPlus.Windows;

namespace GraphPhysics.Resources
{
    class MyResources : ResourceBase
    {
        // singleton instance
        public static MyResources Resources { get; private set; }

        static MyResources()
        {
            Resources = new MyResources();
        }

        public override BitmapSource GetImage(string name)
        {
            try
            {
                return GetImage("pack://application:,,,/NodeGraph;component/", name);
            }
            catch
            {
                return null;
            }
        }

        public override Stream GetResource(string name)
        {
            return GetResource("pack://application:,,,/NodeGraph;component/", name);
        }
    }
}
