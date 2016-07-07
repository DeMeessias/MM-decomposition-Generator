//BipartiteGraph.cs
//Defines the Bipartite Graph class

using System;
using System.Collections;
using System.Collections.Generic;

namespace MMDecompositionGenerator.Data_Structures
{
    /// <summary>
    /// Expands the graph class so that the vertices are part of one of two partitions. There are only edges between vertices in different partitions
    /// </summary>
    class BipartiteGraph : Graph
    {
        //Lists representing the two partitions. Together they contain all vertices
        public BitArray A;

        /// <summary>
        /// Constructor for a bipartite graph. This should ideally only be called by FromPartition, as this guarantees the correct partitioning of vertices
        /// </summary>
        public BipartiteGraph() : base()
        {
            //Initialize the partition lists
            A = new BitArray(Program.numverts);
        }

        /// <summary>
        /// Generates a bipartite graph by dividing up the vertices of an existing graph and only retaining the edges between the two partitions
        /// </summary>
        /// <param name="g">The graph we want to convert</param>
        /// <param name="A">One of the two partitions (the other partition is equal to the rest of the vertices of g)</param>
        /// <returns>A bipartite graph</returns>
        public static BipartiteGraph FromPartition(Graph g, BitArray A)
        {
            //Initialize an empty graph
            var newGraph = new BipartiteGraph();
            newGraph.A = new BitArray(A);
            var verts = new Dictionary<int, Vertex>();

            //Copy the vertices and add them to the correct partition
            foreach (Vertex v in g.vertices.Values)
            {
                var nv = new Vertex(v.Index, v.BitIndex);
                newGraph.vertices.Add(nv.Index,nv);

                if (A[v.BitIndex])
                {
                    nv.A = 1;
                    verts.Add(nv.Index, nv);
                }
                else {
                    nv.A = -1;
                    verts.Add(nv.Index, nv);
                }
            }
            
            //Copy edges as long as they are between vertices of different partitions
            int edges = 0;
            foreach (Edge e in g.edges)
            {
                if (verts[e.u.Index].A != verts[e.v.Index].A)
                {
                    edges += 1;
                    newGraph.ConnectVertices(verts[e.u.Index], verts[e.v.Index]);
                }
            }
            if (edges != newGraph.edges.Count || newGraph.edges.Count > g.edges.Count)
                throw new Exception("Error constructing bipartite graph");

            return newGraph;

        }
    }
}
