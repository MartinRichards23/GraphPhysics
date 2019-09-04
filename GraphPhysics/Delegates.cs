using GraphPhysics.Model;
using System.Windows.Input;

namespace GraphPhysics
{
    public delegate void EdgeAddedHandler (NodeBase from , NodeBase to);

    public delegate void GraphItemClickedHandler(GraphItemClickedEventArgs args);

    public class GraphItemClickedEventArgs
    {
        public GraphItemClickedEventArgs(GraphItem item, MouseButtonEventArgs mouseButtonArgs)
        {
            Item = item;
            MouseButtonArgs = mouseButtonArgs;
        }

        public GraphItem Item { get; }
        public MouseButtonEventArgs MouseButtonArgs { get; }
    }
}
