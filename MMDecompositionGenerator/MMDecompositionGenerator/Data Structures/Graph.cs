using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMDecompositionGenerator.Data_Structures
{
    /// <summary>
    /// Represents a connected, simple graph
    /// </summary>
    class Graph
    {
        Dictionary<int,Vertex> vertices;
        Dictionary<Tuple<int,int>,Edge> edges;

        public int V
        { get { return vertices.Count; } }
        public int E
        { get { return edges.Count; } }
    }
}
