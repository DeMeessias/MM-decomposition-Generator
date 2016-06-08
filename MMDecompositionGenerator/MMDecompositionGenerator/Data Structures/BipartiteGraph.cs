//BipartiteGraph.cs
//Defines the Bipartite Graph class

using System;
using System.Collections.Generic;

namespace MMDecompositionGenerator.Data_Structures
{
    /// <summary>
    /// Expands the graph class so that the vertices are part of one of two partitions. There are only edges between vertices in different partitions
    /// </summary>
    class BipartiteGraph : Graph
    {
        //Lists representing the two partitions. Together they contain all vertices
        public List<Vertex> A;
        public List<Vertex> B;

        /// <summary>
        /// Constructor for a bipartite graph. This should ideally only be called by FromPartition, as this guarantees the correct partitioning of vertices
        /// </summary>
        public BipartiteGraph() : base()
        {
            //Initialize the partition lists
            A = new List<Vertex>();
            B = new List<Vertex>();
        }

        /*
        /// <summary>
        /// Connects two vertices, throwing an error if this would violate partitioning constraints
        /// </summary>
        /// <param name="u">One of the two vertices. The parent of v, if applicable</param>
        /// <param name="v">The other of the two vertices, the child of u, if applicable</param>
        public override void ConnectVertices(Vertex u, Vertex v)
        {
            if (A.Contains(u) == A.Contains(v) || B.Contains(u) == B.Contains(v))
                throw new Exception("Cannot connect vertices in the same partition");
            base.ConnectVertices(u, v);
        }
        */

        /// <summary>
        /// Generates a bipartite graph by dividing up the vertices of an existing graph and only retaining the edges between the two partitions
        /// </summary>
        /// <param name="g">The graph we want to convert</param>
        /// <param name="A">One of the two partitions (the other partition is equal to the rest of the vertices of g)</param>
        /// <returns>A bipartite graph</returns>
        public static BipartiteGraph FromPartition(Graph g, List<Vertex> A)
        {
            //Initialize an empty graph
            var newGraph = new BipartiteGraph();
            var verts = new Dictionary<int, Vertex>();

            //Copy the vertices and add them to the correct partition
            foreach (Vertex v in g.vertices.Values)
            {
                var nv = new Vertex(v.Index);
                newGraph.vertices.Add(nv.Index,nv);

                if (A.Contains(v))
                {
                    nv.A = 1;
                    newGraph.A.Add(nv);
                    verts.Add(nv.Index, nv);
                }
                else {
                    nv.A = -1;
                    newGraph.B.Add(nv);
                    verts.Add(nv.Index, nv);
                }
            }
            if (newGraph.vertices.Count != g.vertices.Count || newGraph.A.Count + newGraph.B.Count != g.vertices.Count)
                throw new Exception("Error constructing bipartite graph");

            //Copy edges as long as they are between vertices of different partitions
            int edges = 0;
            foreach (Edge e in g.edges)
            {
                //if (A.Contains(e.u) != A.Contains(e.v))
                if (verts[e.u.Index].A != verts[e.v.Index].A)
                {
                    edges += 1;

                    //var dummies = new List<Vertex>();
                    //dummies.Add(e.u);
                    //dummies.Add(e.v);
                    //var connectverts = newGraph.vertices.Intersect(dummies).ToList();
                    //if (connectverts.Count != 2 || (connectverts[0].Index != e.u.Index && connectverts[0].Index != e.v.Index) || (connectverts[1].Index != e.v.Index && connectverts[1].Index != e.u.Index) || connectverts[0].Index == connectverts[1].Index)
                    //    throw new Exception("Error constructing bipartite graph");
                    //newGraph.ConnectVertices(connectverts[0], connectverts[1]);
                    newGraph.ConnectVertices(verts[e.u.Index], verts[e.v.Index]);
                }
            }
            if (edges != newGraph.edges.Count || newGraph.edges.Count > g.edges.Count)
                throw new Exception("Error constructing bipartite graph");

            return newGraph;

        }
    }
}
