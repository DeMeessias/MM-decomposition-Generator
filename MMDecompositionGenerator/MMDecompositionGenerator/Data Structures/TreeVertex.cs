//TreeVertex.cs
//Defines the vertices used by the decomposition trees
using System;
using System.Collections.Generic;
using System.Collections;
using MMDecompositionGenerator.Algorithms;
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
        public BitArray bijectedVertices;

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
            bijectedVertices = new BitArray(Program.numverts);
        }

        /// <summary>
        /// Compares this vertex to another one
        /// </summary>
        /// <param name="other">The other tree vertex</param>
        /// <returns>True if the vertices have the same index, false otherwise</returns>
        public bool Equals(TreeVertex other)
        {
            return new PartComparer().Equals(bijectedVertices, other.bijectedVertices);
        }

        /// <summary>
        /// Returns the index of this vertex as a hashcode
        /// </summary>
        /// <returns>The hashcode of this vertex</returns>
        public override int GetHashCode()
        {
            return new PartComparer().GetHashCode(bijectedVertices);
        }
    }
}
