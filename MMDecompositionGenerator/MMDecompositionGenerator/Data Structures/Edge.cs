//Edge.cs
//Defines the edges that are used by the graph class
//Uses Quickgraph (http://quickgraph.codeplex.com/)

using System;
using QuickGraph;

namespace MMDecompositionGenerator.Data_Structures
{
    /// <summary>
    /// Represents an edge between two vertices. Depending on where it is used the edge is directed or undirected
    /// </summary>
    class Edge : IEquatable<Edge>, IEdge<Vertex> //Implements the IEdge interface used by Quickgraph
    {
        //The two vertices connected by the edge
        public Vertex u, v;

        //One of the two vertices connected. If the edge is directed this is the origin vertex
        Vertex IEdge<Vertex>.Source
        {
            get
            {
                return u;
            }
        }

        //One of the two vertices connected. If the edge is directed this is the destination vertex
        Vertex IEdge<Vertex>.Target
        {
            get
            {
                return v;
            }
        }

        /// <summary>
        /// Makes a new Edge object.
        /// </summary>
        /// <param name="u">The first vertex</param>
        /// <param name="v">The second vertex</param>
        public Edge(Vertex u, Vertex v)
        {
                this.u = u;
                this.v = v;
        }

        /// <summary>
        /// Flips the edge, making the source the target & vice versa
        /// </summary>
        public void Flip()
        {
            Vertex x = u;
            u = v;
            v = x;
        }

        /// <summary>
        /// Compares this edge with another. They are considered equal if they connect vertices with the same index
        /// </summary>
        /// <param name="other">The edge to compare this edge to</param>
        /// <returns>True if the edges are equal, false otherwise</returns>
        public bool Equals(Edge other)
        {
            if (u == other.u && v == other.v)
                return true;
            if (u == other.v && v == other.u)
                return true;
            return false;
        }
        
        /// <summary>
        /// Gives a Hashcode based on the edges that this edge connects
        /// </summary>
        /// <returns>a hashcode</returns>
        public override int GetHashCode()
        {
            int hc = 0;
            int a = Math.Max(u.Index, v.Index);
            int b = Math.Min(u.Index, v.Index);
            hc += a * 1000000;
            hc += b;
            return hc;
        }
    }
}
