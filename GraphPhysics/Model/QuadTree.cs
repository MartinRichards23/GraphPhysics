using System;
using System.Collections.Generic;
using System.Windows;
using SystemPlus;

namespace GraphPhysics.Model
{
    public class QuadTree
    {
        #region Fields

        int width = 40;
        int height = 40;
        double quadrantLength = 1500;
        Quadrant[,] quadrants;

        #endregion

        public QuadTree()
        {
            // create the quadrants
            quadrants = new Quadrant[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Rect r = new Rect(x * quadrantLength, y * quadrantLength, quadrantLength, quadrantLength);
                    quadrants[x, y] = new Quadrant(x, y, r);
                }
            }

            // set the neigbours
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Quadrant quad = quadrants[x, y];

                    quad.north = TryGetQuadrant(x, y - 1);
                    quad.northEast = TryGetQuadrant(x + 1, y - 1);
                    quad.east = TryGetQuadrant(x + 1, y);
                    quad.southEast = TryGetQuadrant(x + 1, y + 1);
                    quad.south = TryGetQuadrant(x, y + 1);
                    quad.southWest = TryGetQuadrant(x - 1, y + 1);
                    quad.west = TryGetQuadrant(x - 1, y);
                    quad.northWest = TryGetQuadrant(x - 1, y - 1);
                }
            }
        }

        #region Properties

        public int Width
        {
            get { return width; }
        }

        public int Height
        {
            get { return height; }
        }

        internal Quadrant[,] Quadrants
        {
            get { return quadrants; }
            set { quadrants = value; }
        }

        #endregion

        #region Methods

        public void AddNode(NodeBase node)
        {
            Quadrant quad = GetQuadrant(node.Position);
            node.CurrentQuadrant = quad;
            quad.AddNode(node);
        }

        public void UpdateNodeQuadrant(NodeBase node)
        {
            Quadrant quad = GetQuadrant(node.Position);

            if (quad != node.CurrentQuadrant)
            {
                node.CurrentQuadrant.RemoveNode(node);
                node.CurrentQuadrant = quad;
                quad.AddNode(node);
            }
        }

        public void Clear()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Quadrant q = quadrants[x, y];
                    q.Clear();
                }
            }
        }

        /// <summary>
        /// Gets a quadrant at given index, returns null if not valid index
        /// </summary>
        public Quadrant TryGetQuadrant(int x, int y)
        {
            if (x < 0 || x >= width)
                return null;
            if (y < 0 || y >= height)
                return null;

            return quadrants[x, y];
        }

        /// <summary>
        /// Gets the quadrant that contains the given point
        /// </summary>
        public Quadrant GetQuadrant(Point p)
        {
            int x = (int)Math.Floor(p.X / quadrantLength);
            int y = (int)Math.Floor(p.Y / quadrantLength);

            // clip to bounds of quadrants
            x = MathTools.Clip(x, 0, width - 1);
            y = MathTools.Clip(y, 0, height - 1);

            return quadrants[x, y];
        }

        /// <summary>
        /// Efficiently gets all the nodes in a given rectangle
        /// </summary>
        public IEnumerable<NodeBase> GetNodesInRect(Rect rectangle)
        {
            Quadrant topLeftQuad = GetQuadrant(rectangle.TopLeft);
            Quadrant bottomRightQuad = GetQuadrant(rectangle.BottomRight);
            
            for (int x = topLeftQuad.X; x <= bottomRightQuad.X; x++)
            {
                for (int y = topLeftQuad.Y; y <= bottomRightQuad.Y; y++)
                {
                    Quadrant q = quadrants[x, y];

                    foreach (NodeBase n in q.Nodes)
                    {
                        if (rectangle.Contains(n.Position))
                            yield return n;
                    }

                }
            }
        }

        /// <summary>
        /// Enumerates all quadrants
        /// </summary>
        public IEnumerable<Quadrant> AllQuadrants()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Quadrant q = quadrants[x, y];
                    yield return q;
                }
            }
        }

        /// <summary>
        /// Enumerates all nodes
        /// </summary>
        public IEnumerable<NodeBase> AllNodes()
        {
            foreach (Quadrant q in AllQuadrants())
            {
                for (int i = q.Nodes.Count -1; i >= 0; i--)
                {
                    NodeBase n = q.Nodes[i];
                    yield return n;
                }
            }
        }

        /// <summary>
        /// Returns the centre of the quad tree
        /// </summary>
        public Point GetCentre()
        {
            double totalWidth = width * quadrantLength;
            double totalHeight = height * quadrantLength;

            return new Point(totalWidth / 2, totalHeight / 2);
        }

        #endregion
    }
}
