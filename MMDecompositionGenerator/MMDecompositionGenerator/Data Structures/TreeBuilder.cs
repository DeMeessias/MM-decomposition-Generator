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
        public enum NeighborhoodOperator { cousinSwap, uncleSwap, Sharmin, twoswap }

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
                    throw new NotImplementedException("Heuristic not implemented");
            }
            //Improve the tree with a metaheuristic using the given neighborhood operator
            switch (op)
            {
                case NeighborhoodOperator.uncleSwap:
                    break;
                case NeighborhoodOperator.Sharmin:
                    break;
                default:
                    throw new NotImplementedException("Neighborhood operator not implemented");
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
            //Unless the splitvertex is the root, we add 1/2 of the bijected vertices to the partition at random
            if (splitvert.bijectedVertices.Count != g.vertices.Count)
            {
                while (part.Count < Math.Ceiling(((float)splitvert.bijectedVertices.Count) / 2))
                {
                    var leftoververts = splitvert.bijectedVertices.Except(part).ToList();
                    int rn = r.Next(leftoververts.Count);
                    part.Add(leftoververts[rn]);
                    if (part.Count >= Math.Ceiling(((float)splitvert.bijectedVertices.Count) / 3) && (splitvert.bijectedVertices.Count - part.Count) >= Math.Ceiling(((float)splitvert.bijectedVertices.Count) / 3))
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
                if (mmw < bestmmw && part.Count >= Math.Ceiling(((float)splitvert.bijectedVertices.Count) / 3) && (splitvert.bijectedVertices.Count - part.Count ) >= Math.Ceiling(((float)splitvert.bijectedVertices.Count)/3))
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
        /// <param name="g">The graph we are decomposing</param>
        /// <param name="t">The tree we want to get a neighbor from</param>
        /// <param name="op">The neighborhood operator</param>
        /// <returns>A neighbor solution of t</returns>
        public static Tree getNeighbor(Graph g, Tree t, NeighborhoodOperator op)
        {
            switch (op)
            {
                case NeighborhoodOperator.uncleSwap:
                    return _uncleSwap(t);
                case NeighborhoodOperator.Sharmin:
                    return _SharminNeighbor(g,t);
                case NeighborhoodOperator.twoswap:
                    return _twoSwap(t);
                default:
                    throw new NotImplementedException("Neighborhood operator not implemented");
            }
        }

        /// <summary>
        /// Applies the chosen neighborhood operator to the tree at each point, and then returns the best neighbor in the neighborhood
        /// </summary>
        /// <param name="g">The graph we are decomposing</param>
        /// <param name="t">The initial solution</param>
        /// <param name="op">The neighborhood operator</param>
        /// <returns>The best neighbor solution of t (can actually be worse than t, if t is locally optimal)</returns>
        public static Tree getBestNeighbor(Graph g, Tree t, NeighborhoodOperator op)
        {
            switch (op)
            {
                case NeighborhoodOperator.uncleSwap:
                    return _bestUncleSwap(g, t);
                default:
                    throw new NotImplementedException("Neighborhood operator not implemented");
            }
        }

        private static Tree _twoSwap(Tree t)
        {
            //Get two random tree vertices that are not descendants of each other
            var neighbor = copyExisting(t);
            var rand = new Random();
            int rn1 = 0;
            int rn2 = 0;
            while (rn1 == rn2 || neighbor.Vertices[rn1].Descendants.Contains(neighbor.Vertices[rn2]) || neighbor.Vertices[rn2].Descendants.Contains(neighbor.Vertices[rn1]))
            {
                rn1 = rand.Next(neighbor.Vertices.Count);
                rn2 = rand.Next(neighbor.Vertices.Count);
            }
            var va = neighbor.Vertices[rn1];
            var vb = neighbor.Vertices[rn2];
            //Swap the two vertices
            var vapar = va.parent;
            var vbpar = vb.parent;
            neighbor.DisconnectChild(vapar, va);
            neighbor.DisconnectChild(vbpar, vb);
            neighbor.ConnectChild(vapar, vb);
            neighbor.ConnectChild(vbpar, va);
            var ancest = vapar;
            while (ancest != null)
            {
                ancest.bijectedVertices = ancest.bijectedVertices.Except(va.bijectedVertices).ToList();
                ancest = ancest.parent;
            }
            ancest = vbpar;
            while (ancest != null)
            {
                ancest.bijectedVertices = ancest.bijectedVertices.Except(vb.bijectedVertices).Union(va.bijectedVertices).ToList();
                ancest = ancest.parent;
            }
            ancest = vapar;
            while (ancest != null)
            {
                ancest.bijectedVertices = ancest.bijectedVertices.Union(vb.bijectedVertices).ToList();
                ancest = ancest.parent;
            }

            return neighbor;
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
                    

                    _swapUncle(neighbor,v);
                    
                    return neighbor;
                }
            }
            return neighbor;
        }

        /// <summary>
        /// Helper method of getBestNeighbor. Applies the uncleSwap neighborhood operator and gets the best possible neighbor
        /// </summary>
        /// <param name="g">The graph we are decomposing</param>
        /// <param name="t"></param>
        /// <returns></returns>
        private static Tree _bestUncleSwap(Graph g, Tree t)
        {
            var alg = new Algorithms.Hopcroft_Karp();
            Tree bestSolution = null;
            bool improvedsolution = false;
            int originalMMw = Algorithms.Hopcroft_Karp.GetMMWidth(g, t);
            int bestMMw = int.MaxValue;
            foreach (TreeVertex v in t.Vertices)
            {
                //Check if the vertex has an uncle
                if (v.parent != null && v.parent.parent != null)
                {
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
                    //Check if the swap could possibly reduce the total MM-width
                    var dadgraph = BipartiteGraph.FromPartition(g, dad.bijectedVertices);
                    var dadMatch = alg.GetMatching(dadgraph);
                    //If the swap can reduce MM-width, or if there is no good solution yet, check if the swap makes an improvement
                    if (dadMatch.Count >= originalMMw || !improvedsolution)
                    {
                        var duncleverts = dad.bijectedVertices.Except(v.bijectedVertices).Union(uncle.bijectedVertices).ToList();
                        var dunclegraph = BipartiteGraph.FromPartition(g, duncleverts);
                        var duncleMatch = alg.GetMatching(dunclegraph);
                        //Check if the swap would improve this part of the tree
                        if (duncleMatch.Count < dadMatch.Count || !improvedsolution)
                        {
                            var neighbor = copyExisting(t);
                            var foo = new List<TreeVertex>();
                            foo.Add(v);
                            var bar = neighbor.Vertices.Intersect(foo).ToList();
                            if (bar.Count != 1)
                                throw new Exception("Error generating neighbor");
                            _swapUncle(neighbor, bar[0]);
                            int neighborMMw = Algorithms.Hopcroft_Karp.GetMMWidth(g, neighbor);
                            if (neighborMMw < bestMMw)
                            {
                                if (neighborMMw <= originalMMw)
                                improvedsolution = true;
                                bestSolution = neighbor;
                                bestMMw = neighborMMw;
                            }
                        }
                    }
                }
               
            }
            if (bestSolution == null)
                throw new Exception("Error finding a neighbor");
            return bestSolution;
        }
        
        /// <summary>
        /// Helper method of _uncleSwap and _bestUncleSwap. Swaps a tree vertex with its "uncle".
        /// </summary>
        /// <param name="t">The tree</param>
        /// <param name="granddad">The parent of the parent of the vertex</param>
        /// <param name="dad">The parent of the vertex</param>
        /// <param name="v">The vertex to swap</param>
        /// <param name="uncle">The other vertex to swap</param>
        private static void _swapUncle(Tree t, TreeVertex v)
        {
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
            t.DisconnectChild(granddad, uncle);
            t.DisconnectChild(dad, v);
            t.ConnectChild(granddad, v);
            t.ConnectChild(dad, uncle);
            dad.bijectedVertices = dad.bijectedVertices.Except(v.bijectedVertices).Union(uncle.bijectedVertices).ToList();
        }

        /// <summary>
        /// Helper method of getNeighbor. Applies the Sharmin neighborhood operator
        /// </summary>
        /// <param name="g">The graph we are decomposing</param>
        /// <param name="t">The tree to apply the operator to</param>
        /// <returns>A neighbor solution of t</returns>
        private static Tree _SharminNeighbor(Graph g, Tree t)
        {
            var neighbor = copyExisting(t);
            var root = neighbor.getRoot();
            _TryToImproveSubtree(g,neighbor,root);
            return neighbor;
        }

        /// <summary>
        /// Helper method of _SharminNeighbor. Recursively tries to improve a subtree of the tree decomposition
        /// </summary>
        /// <param name="g">The graph we are decomposing</param>
        /// <param name="t">The tree decomposition</param>
        /// <param name="r">The root of the subtree we want to improve</param>
        private static void _TryToImproveSubtree(Graph g, Tree t, TreeVertex r)
        {
            if (r.bijectedVertices.Count <= 1)
                throw new Exception("Only subtrees with more than 1 bijected vertex can be improved");
            var alg = new Algorithms.Hopcroft_Karp();
            List<Vertex> A, B;
            if (r.children.Count == 0)
                A = _Split(g, r, alg);
            else
            {
                if (r.children.Count != 2)
                    throw new Exception("Tree is not binary");
                A = _RandomSwap(r.children[0], r.children[1]);
            }
            B = r.bijectedVertices.Except(A).ToList();
            //Disconnect all of the old tree vertices that will be replaced
            if (r.children.Count != 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    var v = r.children[0];
                    var Q = new Queue<TreeVertex>();
                    Q.Enqueue(v);
                    while (Q.Count != 0)
                    {
                        v = Q.Dequeue();
                        t.DisconnectChild(v.parent, v);
                        foreach (TreeVertex w in v.children)
                            Q.Enqueue(w);
                        t.Vertices.Remove(v);
                    }
                }
            }
            if (r.children.Count != 0)
                throw new Exception("Error trying to improve subtree");
            //var MMa = alg.GetMatching(BipartiteGraph.FromPartition(g, A));
            //var MMb = alg.GetMatching(BipartiteGraph.FromPartition(g, B));
            var va = new TreeVertex();
            va.bijectedVertices = A;
            t.Vertices.Add(va);
            t.ConnectChild(r, va);
            var vb = new TreeVertex();
            vb.bijectedVertices = B;
            t.Vertices.Add(vb);
            t.ConnectChild(r, vb);
            if (A.Count > 1)
                _TryToImproveSubtree(g, t, va);
            if (B.Count > 1)
                _TryToImproveSubtree(g, t, vb);


        }

        /// <summary>
        /// Helper method of _TryToImproveSubtree, randomly exchanges some bijected vertices of two tree vertices
        /// </summary>
        /// <param name="x">The first treevertex to have its bijected vertices swapped</param>
        /// <param name="y">The second treevertex to have its bijected vertices swapped</param>
        /// <returns>The first part of the new partition of the combined bijected vertices made</returns>
        private static List<Vertex> _RandomSwap(TreeVertex y, TreeVertex z)
        {
            var A = new List<Vertex>();
            var Py = new List<Vertex>();
            Py.AddRange(y.bijectedVertices);
            var Pz = new List<Vertex>();
            Pz.AddRange(z.bijectedVertices);
            var rand = new Random();
            int i = rand.Next(Py.Count - (y.parent.bijectedVertices.Count / 3));
            int j = rand.Next(Pz.Count - (z.parent.bijectedVertices.Count / 3));
            var Mi = new List<Vertex>();
            var Mj = new List<Vertex>();
            for (int k = 0; k < i; k++)
            {
                int rn = rand.Next(Py.Count);
                Mi.Add(Py[rn]);
                Py.RemoveAt(rn);
            }
            for (int k = 0; k < j; k++)
            {
                int rn = rand.Next(Pz.Count);
                Mj.Add(Pz[rn]);
                Pz.RemoveAt(rn);
            }
            if (Mi.Count != i || Mj.Count != j || Py.Count != y.bijectedVertices.Count - i || Pz.Count != z.bijectedVertices.Count - j)
                throw new Exception("Error performing randomswap");
            A = y.bijectedVertices.Except(Mi).Union(Mj).ToList();
            return A;
        }

        /// <summary>
        /// Uses simulated annealing to try and improve a tree decomposition
        /// </summary>
        /// <param name="g">The graph the tree is a decomposition of</param>
        /// <param name="T">Our initial solution</param>
        /// <param name="op">The neighborhood operator</param>
        /// <param name="startTemperature">The starting temperature</param>
        /// <param name="decreaseIterations">How many times we should iterate before decreasing the temperature</param>
        /// <param name="tempMultiplier">Multiplier that determines how fast the temperature decreases, should be between 0 and 1 (exclusive), usually close to 1</param>
        /// <param name="msToRun">How many milliseconds we should run SA before returning a solution</param>
        /// <returns>The best tree we have found while running SA</returns>
        public static Tree SimulatedAnnealing(Graph g, Tree T, NeighborhoodOperator op, float startTemperature, int decreaseIterations, float tempMultiplier, double msToRun)
        {
            var starttime = DateTime.Now;
            var endtime = starttime.AddMilliseconds(msToRun);
            float temperature = startTemperature;
            var currentSolution = T;
            var bestSolution = T;
            int currMMw = Algorithms.Hopcroft_Karp.GetMMWidth(g, T);
            int bestMMw = currMMw;
            var rand = new Random();
            int iterations = 0;
            while (DateTime.Now < endtime)
            {
                var neighbor = getNeighbor(g, currentSolution, op);
                int nMMw = Algorithms.Hopcroft_Karp.GetMMWidth(g, neighbor);
                if (nMMw < bestMMw)
                {
                    bestMMw = nMMw;
                    bestSolution = neighbor;
                }

                if (nMMw < currMMw)
                {
                    currentSolution = neighbor;
                    currMMw = nMMw;
                }
                else
                {
                    double p = Math.Exp((currMMw - nMMw) / temperature);
                    Console.WriteLine(nMMw - currMMw);
                    Console.WriteLine(p);
                    var rn = rand.NextDouble();
                    if (rn < p)
                    {
                        currentSolution = neighbor;
                        currMMw = nMMw;
                    }
                }
                iterations++;
                if (iterations % decreaseIterations == 0)
                temperature *= tempMultiplier;
            }
            return bestSolution;
        }

        /// <summary>
        /// Improves the solution by continually taking the best neighbor until this is no longer an improvement
        /// </summary>
        /// <param name="g">the graph we are decomposing</param>
        /// <param name="T">Our initial solution</param>
        /// <param name="op">The neighborhood operator</param>
        /// <param name="msToRun">After how many milliseconds we should time out and return the best solution found so far</param>
        /// <returns>A solution that is a local optimum with respect to the neighborhood operator</returns>
        public static Tree IteratedLocalSearch(Graph g, Tree T, NeighborhoodOperator op, double msToRun)
        {
            var starttime = DateTime.Now;
            var endtime = starttime.AddMilliseconds(msToRun);
            var bMMw = Algorithms.Hopcroft_Karp.GetMMWidth(g,T);
            var best = T;
            var neighbor = getBestNeighbor(g, T, op);
            var nMMw = Algorithms.Hopcroft_Karp.GetMMWidth(g, neighbor);
            while (nMMw < bMMw && DateTime.Now < endtime)
            {
                bMMw = nMMw;
                best = neighbor;
                neighbor = getBestNeighbor(g, neighbor, op);
                nMMw = Algorithms.Hopcroft_Karp.GetMMWidth(g, neighbor);            
            }
            if (nMMw < bMMw)
                return neighbor;
            else return best;
        }
    }
}
