﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GraphPhysics.Model
{
    public class Quadrant
    {
        #region Fields

        readonly int x;
        readonly int y;
        readonly Rect rect;

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
            this.x = x;
            this.y = y;
            this.rect = rect;
        }

        #region Properties

        public int X
        {
            get { return x; }
        }

        public int Y
        {
            get { return y; }
        }

        public Rect Rect
        {
            get { return rect; }
        }

        public IList<NodeBase> Nodes
        {
            get { return nodes; }
        }

        #endregion

        #region Methods

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
            return string.Format("x={0}, y={1}", x, y);
        }

        #endregion
    }
}
