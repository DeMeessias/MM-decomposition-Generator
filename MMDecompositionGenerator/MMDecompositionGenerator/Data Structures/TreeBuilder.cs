//TreeBuilder.cs
//Defines the treebuilder class
using System;
using System.Collections.Generic;
using System.Linq;

namespace MMDecompositionGenerator.Data_Structures
{
    /// <summary>
    /// Used to build tree decompositions that have small MM-width
    /// </summary>
    class TreeBuilder
    {
        //Possible construction heuristics for building trees
        public enum Heuristic { tSharmin, bAllpairs, bRandomGreedy, bSmallest, bcompletelyRandom, lbRandom, lbGreedy }
        //Possible neighborhood operators for improving a tree using Local Search
        public enum NeighborhoodOperator { nephewSwap, uncleSwap, Sharmin }

        /// <summary>
        /// Makes a copy of an existing tree
        /// </summary>
        /// <param name="t">The tree we want to copy</param>
        /// <returns>A tree identical to the original one</returns>
        public static Tree copyExisting(Tree t)
        {
            var copy = new Tree();
            //Make new vertices for all vertices in the original tree
            foreach (TreeVertex v in t.Vertices)
            {
                var tv = new TreeVertex();
                foreach (Vertex bv in v.bijectedVertices)
                    tv.bijectedVertices.Add(bv);
                copy.Vertices.Add(tv);
            }
            //Make new edges for the edges in the original tree
            foreach (TreeEdge e in t.Edges)
            {
                var dummies = new List<TreeVertex>();
                dummies.Add(e.Source);
                dummies.Add(e.Target);
                var connectverts = copy.Vertices.Intersect(dummies).ToList();
                if (connectverts.Count != 2)
                    throw new Exception("Error copying tree");
                if (connectverts[0].Index != e.Source.Index)
                {
                    var foo = connectverts[0];
                    connectverts[0] = connectverts[1];
                    connectverts[1] = foo;
                }
                if (connectverts[0].Index != e.Source.Index || connectverts[1].Index != e.Target.Index)
                    throw new Exception("Error copying tree");
                var te = new TreeEdge(connectverts[0], connectverts[1]);
                copy.Edges.Add(te);
                connectverts[0].incedentEdges.Add(te);
                connectverts[1].incedentEdges.Add(te);
                connectverts[0].neighbors.Add(connectverts[1]);
                connectverts[1].neighbors.Add(connectverts[0]);
                connectverts[0].children.Add(connectverts[1]);
                connectverts[1].parent = connectverts[0];
            }
            return copy;
        }

        /// <summary>
        /// Heuristically generate a tree decomposition of a graph that has a small MM-width
        /// </summary>
        /// <param name="g">The graph we want to decompose</param>
        /// <param name="h">The construction heuristic to be used</param>
        /// <param name="op">The neighborhood operator to be used by local search</param>
        /// <param name="preprocess">Whether or not we want to preprocess by removing all isolated and pendant vertices</param>
        /// <returns>A tree decomposition of g</returns>
        public static Tree fromGraph(Graph g, Heuristic h, NeighborhoodOperator op, bool preprocess)
        {
            var removedVertices = new List<Vertex>();
            Tree decomposition;
            //Preprocess
            if (preprocess)
            {
                //make a copy of the original graph
                Graph og = g;
                g = new Graph();
                foreach (Vertex v in og.vertices)
                    g.vertices.Add(new Vertex(v.Index));
                foreach (Edge e in og.edges)
                {
                    var dummies = new List<Vertex>();
                    dummies.Add(e.u);
                    dummies.Add(e.v);
                    var connectverts = g.vertices.Intersect(dummies).ToList();
                    if (connectverts.Count != 2 || (connectverts[0].Index != e.u.Index && connectverts[0].Index != e.v.Index) || (connectverts[1].Index != e.v.Index && connectverts[1].Index != e.u.Index) || connectverts[0].Index == connectverts[1].Index)
                        throw new Exception("Error trying to duplicate graph");

                    //var eu = g.Vertices[e.u.Index - 1];
                    //var ev = g.Vertices[e.v.Index - 1];
                    //if (eu.Index != e.u.Index || ev.Index != e.v.Index)
                    // throw new Exception("Error trying to duplicate graph");
                    var f = new Edge(connectverts[0], connectverts[1]);
                    g.edges.Add(f);
                    connectverts[0].incedentEdges.Add(f);
                    connectverts[1].incedentEdges.Add(f);
                    connectverts[0].neighbors.Add(connectverts[1]);
                    connectverts[1].neighbors.Add(connectverts[0]);
                }
                //Remove all vertices with degree less than 2
                var S = new Stack<Vertex>();
                foreach (Vertex v in g.vertices)
                    S.Push(v);
                while (S.Count > 0)
                {
                    var v = S.Pop();
                    if (v.neighbors.Count < 2 && !removedVertices.Contains(v) && g.edges.Count > 1)
                    {
                        g.vertices.Remove(v);
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
            }

            //Construct an initial tree using the given heuristic
            switch (h)
            {
                case Heuristic.bAllpairs:
                case Heuristic.bRandomGreedy:
                case Heuristic.bSmallest:
                case Heuristic.bcompletelyRandom:
                    decomposition = _fromGraphBottomUp(g, h);
                    break;
                case Heuristic.tSharmin:
                    decomposition = _fromGraphTopDown(g, h);
                    break;
                default:
                    throw new NotImplementedException();
            }
            //Improve the tree with a metaheuristic using the given neighborhood operator
            switch (op)
            {
                case NeighborhoodOperator.uncleSwap:
                    break;
            }
            //If the graph was preprocessed, re-add the removed vertices.
            if (preprocess)
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

                        foreach (TreeVertex tv in decomposition.Vertices)
                        {
                            if (tv.bijectedVertices.Count == 1 && tv.bijectedVertices.Contains(v.neighbors[0]))
                            { n = tv; }
                        }
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

            return decomposition;
        }

        /// <summary>
        /// Builds a tree composition of a graph bottom-up using a construction heuristic.
        /// </summary>
        /// <param name="g">The graph we want to decompose</param>
        /// <param name="h">The heuristic we use to build the tree</param>
        /// <returns>A tree decomposition of g</returns>
        private static Tree _fromGraphBottomUp(Graph g, Heuristic h)
        {
            var alg = new Algorithms.Hopcroft_Karp();
            //var alg = new Algorithms.fastMaximal();
            var topVertices = new List<TreeVertex>();
            var decomposition = new Tree();
            //Add all the graph vertices as leaves in the tree
            foreach (Vertex v in g.vertices)
            {
                var tv = new TreeVertex();
                tv.bijectedVertices.Add(v);
                decomposition.Vertices.Add(tv);
                topVertices.Add(tv);
            }
            //Merge vertices bottom up according to the chosen heuristic until the tree has a single root containing a bijection to all vertices of the original graph
            while (topVertices.Count > 1)
            {
                TreeVertex mv = null;
                TreeVertex ov = null;
                int mmw;
                switch (h)
                {
                    case Heuristic.bRandomGreedy:
                        var r = new Random();
                        int rn = r.Next(topVertices.Count);
                        mv = topVertices[rn];
                        ov = null;
                        mmw = int.MaxValue;
                        for (int i = 0; i < topVertices.Count; i++)
                        {
                            if (i != rn)
                            {
                                var bivertices = new List<Vertex>();
                                bivertices = bivertices.Union(mv.bijectedVertices).ToList();
                                bivertices = bivertices.Union(topVertices[i].bijectedVertices).ToList();
                                var part = BipartiteGraph.FromPartition(g, bivertices);
                                int mw = alg.GetMatching(part).Count;
                                if (mw < mmw)
                                {
                                    ov = topVertices[i];
                                    mmw = mw;
                                }
                            }
                        }

                        break;
                    case Heuristic.bSmallest:
                        mv = topVertices[0];
                        foreach (TreeVertex topv in topVertices)
                        {
                            if (topv.bijectedVertices.Count < mv.bijectedVertices.Count)
                                mv = topv;
                        }
                        mmw = int.MaxValue;
                        for (int i = 0; i < topVertices.Count; i++)
                        {
                            if (topVertices[i] != mv)
                            {
                                var bivertices = new List<Vertex>();
                                bivertices = bivertices.Union(mv.bijectedVertices).ToList();
                                bivertices = bivertices.Union(topVertices[i].bijectedVertices).ToList();
                                var part = BipartiteGraph.FromPartition(g, bivertices);
                                int mw = alg.GetMatching(part).Count;
                                if (mw < mmw)
                                {
                                    ov = topVertices[i];
                                    mmw = mw;
                                }
                            }
                        }
                        break;
                    case Heuristic.bAllpairs:
                        mmw = int.MaxValue;
                        for (int i = 0; i < topVertices.Count - 1; i++)
                            for (int j = 1; j < topVertices.Count; j++)
                            {
                                if (i < j)
                                {
                                    var bivertices = new List<Vertex>();
                                    bivertices = bivertices.Union(topVertices[i].bijectedVertices).ToList();
                                    bivertices = bivertices.Union(topVertices[j].bijectedVertices).ToList();
                                    var part = BipartiteGraph.FromPartition(g, bivertices);
                                    int mw = alg.GetMatching(part).Count;
                                    if (mw < mmw)
                                    {
                                        mv = topVertices[i];
                                        ov = topVertices[j];
                                        mmw = mw;
                                    }
                                }
                            }
                        break;
                    case Heuristic.bcompletelyRandom:
                        var rr = new Random();
                        int randn = rr.Next(topVertices.Count);
                        int randn2 = randn;
                        while (randn2 == randn)
                            randn2 = rr.Next(topVertices.Count);
                        mv = topVertices[randn];
                        ov = topVertices[randn2];
                        break;
                    default:
                        throw new Exception("Invalid Heuristic");
                }
                var nv = new TreeVertex();
                decomposition.Vertices.Add(nv);
                decomposition.ConnectChild(nv, mv);
                decomposition.ConnectChild(nv, ov);
                topVertices.Remove(mv);
                topVertices.Remove(ov);
                topVertices.Add(nv);
                nv.bijectedVertices = mv.bijectedVertices.Union(ov.bijectedVertices).ToList();
            }
            return decomposition;
        }

        /// <summary>
        /// Builds a tree decomposition of a graph top down using a construction heuristic
        /// </summary>
        /// <param name="g">The graph we want to decompose</param>
        /// <param name="h">The construction heuristic we want to use</param>
        /// <returns>A Tree decomposition of g</returns>
        private static Tree _fromGraphTopDown(Graph g, Heuristic h)
        {
            var alg = new Algorithms.Hopcroft_Karp();
            var decomposition = new Tree();
            var root = new TreeVertex();
            var S = new Stack<TreeVertex>();
            //Create a root vertex that has a bijection to all of the vertices of g
            root.bijectedVertices = root.bijectedVertices.Union(g.vertices).ToList();
            decomposition.Vertices.Add(root);
            //Split the top vertices into partitions until a full decomposition is made.
            S.Push(root);
            while (S.Count > 0)
            {
                var v = S.Pop();
                if (v.bijectedVertices.Count > 1)
                {
                    var a = _Split(g, v, alg);
                    var b = v.bijectedVertices.Except(a).ToList();
                    var lc = new TreeVertex();
                    var rc = new TreeVertex();
                    lc.bijectedVertices = a;
                    rc.bijectedVertices = b;
                    decomposition.Vertices.Add(lc);
                    decomposition.Vertices.Add(rc);
                    decomposition.ConnectChild(v, lc);
                    decomposition.ConnectChild(v, rc);
                    S.Push(lc);
                    S.Push(rc);
                }
                else if (v.bijectedVertices.Count != 1)
                    throw new Exception("Error constructing tree");
            }
            int vertcheck = 2 * g.vertices.Count - 1;
            if (decomposition.Vertices.Count != vertcheck)
                throw new Exception("Error constructing tree");
            return decomposition;
        }

        /// <summary>
        /// Helper method for _fromGraphTopDown using the tSharmin heuristic. Splits the set of bijected vertices of a tree vertex into two parts to be turned into a subtree
        /// </summary>
        /// <param name="g">The graph the tree decomposition is based on</param>
        /// <param name="splitvert">The tree vertex to split</param>
        /// <param name="alg">The algorithm used to determine a maximum matching</param>
        /// <returns>A list of bijected vertices in the first part of the best partition found</returns>
        private static List<Vertex> _Split(Graph g, TreeVertex splitvert, Algorithms.Hopcroft_Karp alg)
        {
            int bestmmw = int.MaxValue;
            var bestpart = new List<Vertex>();
            var part = new List<Vertex>();
            var r = new Random();
            //Unless the splitvertex is the root, we add 1/3 of the bijected vertices to the partition at random
            if (splitvert.bijectedVertices.Count != g.vertices.Count)
            {
                while (part.Count < Math.Ceiling(((float)splitvert.bijectedVertices.Count) / 2))
                {
                    var leftoververts = splitvert.bijectedVertices.Except(part).ToList();
                    int rn = r.Next(leftoververts.Count);
                    part.Add(leftoververts[rn]);
                    if (part.Count >= Math.Ceiling(((float)splitvert.bijectedVertices.Count) / 3))
                    {
                        leftoververts.Remove(leftoververts[rn]);
                        var bgraph = BipartiteGraph.FromPartition(g, part);
                        var bgraph2 = BipartiteGraph.FromPartition(g, leftoververts);
                        var foo = Math.Max(alg.GetMatching(bgraph).Count, alg.GetMatching(bgraph2).Count);
                        if (foo < bestmmw)
                        {
                            bestmmw = foo;
                            bestpart = new List<Vertex>().Union(part).ToList();
                        }
                    }
                }
            }
            //Create possible partitions until the remainder of the bijected vertices becomes too small.
            while (splitvert.bijectedVertices.Count - part.Count > Math.Ceiling(((float)splitvert.bijectedVertices.Count) / 3))
            {
                int mmw = int.MaxValue;
                Vertex topv = null;
                foreach (Vertex v in splitvert.bijectedVertices.Except(part).ToList())
                {
                    var A = new List<Vertex>().Union(part).ToList();
                    A.Add(v);
                    var B = splitvert.bijectedVertices.Except(A).ToList();
                    var bgraph = BipartiteGraph.FromPartition(g, A);
                    var bgraph2 = BipartiteGraph.FromPartition(g, B);
                    var foo = Math.Max(alg.GetMatching(bgraph).Count, alg.GetMatching(bgraph2).Count);
                    if (foo < mmw)
                    {
                        mmw = foo;
                        topv = v;
                    }

                }
                if (topv == null)
                    throw new Exception("Error Splitting tree vertex");
                part.Add(topv);
                if (mmw < bestmmw && part.Count >= Math.Ceiling(((float)splitvert.bijectedVertices.Count) / 3))
                {
                    bestmmw = mmw;
                    bestpart = new List<Vertex>().Union(part).ToList();
                }
            }

            //Return the best partition found among the created partitions
            return bestpart;
        }

        /// <summary>
        /// Applies the chosen neighborhood operator to a tree
        /// </summary>
        /// <param name="t">The tree we want to get a neighbor from</param>
        /// <param name="op">The neighborhood operator</param>
        /// <returns>A neighbor solution of t</returns>
        private static Tree getNeighbor(Tree t, NeighborhoodOperator op)
        {
            switch (op)
            {
                case NeighborhoodOperator.uncleSwap:
                    return _uncleSwap(t);
                    break;
                case NeighborhoodOperator.Sharmin:
                    return _SharminNeighbor(t);
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Helper method of getNeighbor. Applies the uncleSwap neighborhood operator
        /// </summary>
        /// <param name="t">The tree to apply the operator to</param>
        /// <returns>A neighbor solution of t, or t, if the tree is too small to apply the operator to.</returns>
        private static Tree _uncleSwap(Tree t)
        {
            var neighbor = copyExisting(t);

            //Find a vertex in the tree that is at least 2 steps away from the root of the tree, if there is one.
            var tabuNums = new List<int>();
            var rando = new Random();
            while (tabuNums.Count < neighbor.Vertices.Count)
            {
                int rn = rando.Next(neighbor.Vertices.Count);
                while (tabuNums.Contains(rn))
                    rn = rando.Next(neighbor.Vertices.Count);
                tabuNums.Add(rn);
                var v = neighbor.Vertices[rn];
                if (v.parent != null && v.parent.parent != null)
                {
                    //Swap the found vertex with the other child of its parent's parent.
                    var dad = v.parent;
                    var granddad = dad.parent;
                    TreeVertex uncle = null;
                    foreach (TreeVertex tv in granddad.children)
                    {
                        if (tv != dad)
                            uncle = tv;
                    }
                    if (uncle == null)
                        throw new Exception("Error during neighbor generation");

                    neighbor.DisconnectChild(granddad, uncle);
                    neighbor.DisconnectChild(dad, v);
                    neighbor.ConnectChild(granddad, v);
                    neighbor.ConnectChild(dad, uncle);
                    dad.bijectedVertices = dad.bijectedVertices.Except(v.bijectedVertices).Union(uncle.bijectedVertices).ToList();
                    return neighbor;
                }
            }
            return neighbor;
        }

        /// <summary>
        /// Helper method of getNeighbor. Applies the Sharmin neighborhood operator
        /// </summary>
        /// <param name="t">The tree to apply the operator to</param>
        /// <returns>A neighbor solution of t</returns>
        private static Tree _SharminNeighbor(Tree t)
        {
            var neighbor = copyExisting(t);

            return neighbor;
        }
    }
}
