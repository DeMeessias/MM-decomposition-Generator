//TreeVertex.cs
//Defines the vertices used by the decomposition trees
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMDecompositionGenerator.Data_Structures
{
    /// <summary>
    /// Represents a vertex in a tree decomposition of a graph
    /// </summary>
    class TreeVertex : IEquatable<TreeVertex>
    {
        public List<TreeEdge> incedentEdges;
        public List<TreeVertex> neighbors;
        public TreeVertex parent;
        public List<TreeVertex> children;
        public List<Vertex> bijectedVertices;

        //The index of the vertex, used to compare two vertices. Based on the bijected vertices of the orginal graph.
        public int Index
        { get {
                if (bijectedVertices.Count == 1)
                    return bijectedVertices[0].Index;
                else
                { int i = 0;
                    foreach (Vertex v in bijectedVertices)
                        i += (int)Math.Pow(v.Index + 100, 2);
                    return i;
                }
            }
        }

        /// <summary>
        /// Property returning all vertices that are descendants of this vertex
        /// </summary>
        public List<TreeVertex> Descendants
        {
            get { var d = new List<TreeVertex>();
                var q = new Queue<TreeVertex>();
                q.Enqueue(this);
                while (q.Count != 0)
                {
                    var v = q.Dequeue();
                    foreach (TreeVertex tv in v.children)
                        q.Enqueue(tv);
                    d.Add(v);
                }
                return d;
            }
        }

        /// <summary>
        /// Constructor initializing the values of a new tree vertex
        /// </summary>
        public TreeVertex()
        {
            incedentEdges = new List<TreeEdge>();
            neighbors = new List<TreeVertex>();
            parent = null;
            children = new List<TreeVertex>();
            bijectedVertices = new List<Vertex>();
        }

        /// <summary>
        /// Compares this vertex to another one
        /// </summary>
        /// <param name="other">The other tree vertex</param>
        /// <returns>True if the vertices have the same index, false otherwise</returns>
        public bool Equals(TreeVertex other)
        {
            return Index == other.Index;
        }

        /// <summary>
        /// Returns the index of this vertex as a hashcode
        /// </summary>
        /// <returns>The hashcode of this vertex</returns>
        public override int GetHashCode()
        {
            return Index;
        }
    }
}
