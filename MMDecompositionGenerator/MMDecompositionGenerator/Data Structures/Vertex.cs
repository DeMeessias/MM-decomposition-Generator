//Vertex.cs
//Defines the vertices used by our graphs.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMDecompositionGenerator.Data_Structures
{
    /// <summary>
    /// Represents a vertex in a graph
    /// </summary>
    class Vertex : IEquatable<Vertex>, IComparable<Vertex>
    {
        int index;
        public List<Edge> incedentEdges;
        public List<Vertex> neighbors;
        public int A; //1 if Vertex is in partition A, -1 if it is in B, 0 if it is not in a bipartite graph;
        
        public int Index { get { return index; } }

        /// <summary>
        /// Creates a new Vertex object with the given index
        /// </summary>
        /// <param name="index">the index of the new vertex</param>
        public Vertex(int index)
        {
            this.index = index;
            incedentEdges = new List<Edge>();
            neighbors = new List<Vertex>();
            A = 0;
        }

        /// <summary>
        /// Compares this vertex to another vertex
        /// </summary>
        /// <param name="other">The vertex to be compared to</param>
        /// <returns>True if the vertices are the same, fals otherwise</returns>
        public bool Equals(Vertex other)
        {
            return (Index == other.Index);
        }

        /// <summary>
        /// Gives a hashcode for this vertex
        /// </summary>
        /// <returns>A hashcode</returns>
        public override int GetHashCode()
        {
            return index;
        }

        /// <summary>
        /// Checks if a vertex has a larger or smaller index than another
        /// </summary>
        /// <param name="other">The other vertex</param>
        /// <returns>The relative ordering of the vertices</returns>
        public int CompareTo(Vertex other)
        {
            return index.CompareTo(other.index);
        }
    }
}
