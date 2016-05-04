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
    class Vertex
    {
        int index;
        List<Edge> incedentEdges;
        List<Vertex> neighbors;
        
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

        public void RemoveConnection(Vertex v)
        {
            neighbors.Remove(v);
            incedentEdges.Remove(new Edge(this, v));
        }

        public void AddConnection(Vertex v)
        {
            neighbors.Add(v);
            incedentEdges.Add(new Edge(this, v));
        }
    }
}
