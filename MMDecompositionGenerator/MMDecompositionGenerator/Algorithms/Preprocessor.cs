//Preprocessor.cs
//Algorithm for preprocessing a graph and converting a tree decomposition made from the preprocessed graph to one for the original graph
using System;
using System.Collections.Generic;
using System.Linq;
using MMDecompositionGenerator.Data_Structures;

namespace MMDecompositionGenerator.Algorithms
{
    /// <summary>
    /// Class for preprocessing graphs to remove some vertices
    /// </summary>
    class Preprocessor
    {
        private List<Vertex> removedVertices;

        /// <summary>
        /// Preprocesses a graph for decomposition, removing all vertices of degree 1 or 0.
        /// </summary>
        /// <param name="og">Our original graph</param>
        /// <returns>A preprocessed graph</returns>
        public Graph preprocessGraph(Graph og)
        {
            removedVertices = new List<Vertex>();
            //make a copy of the original graph
            Graph g = new Graph(og) ;
            
            //Remove all vertices with degree less than 2
            var S = new Stack<Vertex>();
            foreach (Vertex v in g.vertices.Values)
                S.Push(v);
            while (S.Count > 0)
            {
                var v = S.Pop();
                if (v.neighbors.Count < 2 && !removedVertices.Contains(v) && g.edges.Count > 1)
                {
                    g.vertices.Remove(v.Index);
                    removedVertices.Add(v);
                    foreach (Vertex n in v.neighbors)
                    {
                        var e = new Edge(v, n);
                        n.neighbors.Remove(v);
                        n.incedentEdges.Remove(e);
                        g.edges.Remove(e);
                        S.Push(n);
                    }
                }
            }
            Console.WriteLine("Removed " + removedVertices.Count + " vertices during preprocessing");
            return g;
        }

        /// <summary>
        /// Adds the vertices that were removed during preprocessing back into the tree
        /// </summary>
        /// <param name="decomposition">Our tree decomposition</param>
        public void completeTree(Tree decomposition)
        {
                for (int i = removedVertices.Count - 1; i >= 0; i--)
                {
                    var v = removedVertices[i];
                    TreeVertex n = null;
                    var vtv = new TreeVertex();
                    vtv.bijectedVertices.Add(v);
                    //find the leaf the node should be connected to
                    if (v.neighbors.Count == 1)
                    {
                    n = decomposition.findVertex(v.neighbors);
                        //foreach (TreeVertex tv in decomposition.Vertices)
                        //{
                        //    if (tv.bijectedVertices.Count == 1 && tv.bijectedVertices.Contains(v.neighbors[0]))
                        //    { n = tv; }
                        //}
                    }
                    else
                    {
                        foreach (TreeVertex tv in decomposition.Vertices)
                            if (tv.bijectedVertices.Count == 1)
                                n = tv;
                    }
                    if (n == null)
                        throw new Exception("Could not find where to reinsert vertex");
                    var utv = new TreeVertex();
                    utv.bijectedVertices = n.bijectedVertices.Union(vtv.bijectedVertices).ToList();
                    decomposition.Vertices.Add(utv);
                    decomposition.Vertices.Add(vtv);
                    var npar = n.parent;
                    decomposition.DisconnectChild(n.parent, n);
                    decomposition.ConnectChild(utv, n);
                    decomposition.ConnectChild(utv, vtv);
                    decomposition.ConnectChild(npar, utv);
                    var p = utv.parent;
                    while (p != null)
                    {
                        p.bijectedVertices.Add(v);
                        p = p.parent;
                    }
                }
            }
    }
}
