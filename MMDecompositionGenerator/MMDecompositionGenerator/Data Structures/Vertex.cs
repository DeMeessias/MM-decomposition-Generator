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
    class Vertex : IEquatable<Vertex>
    {
        int index;
        public List<Edge> incedentEdges;
        public List<Vertex> neighbors;
        
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
    }
}
