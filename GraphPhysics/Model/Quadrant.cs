using System.Collections.Generic;
using System.Windows;

namespace GraphPhysics.Model
{
    /// <summary>
    /// A quadrant, part of a QuadTree
    /// </summary>
    public class Quadrant
    {
        #region Fields

        readonly List<NodeBase> nodes = new List<NodeBase>();

        // adjacent quadrants
        internal Quadrant north;
        internal Quadrant northEast;
        internal Quadrant east;
        internal Quadrant southEast;
        internal Quadrant south;
        internal Quadrant southWest;
        internal Quadrant west;
        internal Quadrant northWest;

        #endregion

        public Quadrant(int x, int y, Rect rect)
        {
            X = x;
            Y = y;
            Rect = rect;
        }

        #region Properties

        public int X { get; }
        public int Y { get; }
        public Rect Rect { get; }

        public IList<NodeBase> Nodes
        {
            get { return nodes; }
        }

        #endregion

        #region Public methods

        public void AddNode(NodeBase node)
        {
            nodes.Add(node);
        }

        public void RemoveNode(NodeBase node)
        {
            nodes.Remove(node);
        }

        public void Clear()
        {
            nodes.Clear();
        }

        /// <summary>
        /// Enumerates all nodes in the north east, east, south east and south neighbours
        /// </summary>
        public IEnumerable<NodeBase> AdjancentNodes()
        {
            if (northEast != null)
            {
                foreach (Node n in northEast.nodes)
                    yield return n;
            }

            if (east != null)
            {
                foreach (Node n in east.nodes)
                    yield return n;
            }

            if (southEast != null)
            {
                foreach (Node n in southEast.nodes)
                    yield return n;
            }

            if (south != null)
            {
                foreach (Node n in south.nodes)
                    yield return n;
            }
        }

        /// <summary>
        /// Enumerates all nodes in this quadrant and the surrounding quadrants
        /// </summary>
        public IEnumerable<NodeBase> CloseNodes()
        {
            foreach (Node n in nodes)
                yield return n;

            if (north != null)
            {
                foreach (Node n in north.nodes)
                    yield return n;
            }

            if (northEast != null)
            {
                foreach (Node n in northEast.nodes)
                    yield return n;
            }

            if (east != null)
            {
                foreach (Node n in east.nodes)
                    yield return n;
            }

            if (southEast != null)
            {
                foreach (Node n in southEast.nodes)
                    yield return n;
            }

            if (south != null)
            {
                foreach (Node n in south.nodes)
                    yield return n;
            }

            if (southWest != null)
            {
                foreach (Node n in southWest.nodes)
                    yield return n;
            }

            if (west != null)
            {
                foreach (Node n in west.nodes)
                    yield return n;
            }

            if (northWest != null)
            {
                foreach (Node n in northWest.nodes)
                    yield return n;
            }
        }

        public override string ToString()
        {
            return string.Format("x={0}, y={1}", X, Y);
        }

        #endregion
    }
}
