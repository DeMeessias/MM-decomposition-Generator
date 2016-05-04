using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMDecompositionGenerator.Data_Structures
{
    /// <summary>
    /// Represents an undirected edge between two vertices
    /// </summary>
    class Edge : IEquatable<Edge>
    {
        public Vertex u, v;

        /// <summary>
        /// Makes a new Edge object. The "first" vertex in the edge is always the one with the smaller index.
        /// </summary>
        /// <param name="u">The first vertex</param>
        /// <param name="v">The second vertex</param>
        public Edge(Vertex u, Vertex v)
        {
            if (v.Index > u.Index)
            {
                this.u = u;
                this.v = v;
            }
            else
            {
                this.u = v;
                this.v = u;
            }
        }

        public bool Equals(Edge other)
        {
            if (u == other.u && v == other.v)
                return true;
            return false;
        }
        
    }
}
