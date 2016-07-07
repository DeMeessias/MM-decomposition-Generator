//TreeBuilder.cs
//Defines the treebuilder class
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MMDecompositionGenerator.Data_Structures
{
    /// <summary>
    /// Used to build decomposition trees that have small MM-width
    /// </summary>
    class TreeBuilder
    {

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
                tv.bijectedVertices = new BitArray(v.bijectedVertices);
                copy.Vertices.Add(tv);
            }
            //Make new edges for the edges in the original tree
            foreach (TreeEdge e in t.Edges)
            {
                var foo = new List<TreeVertex>();
                foo.Add(e.Source);
                var ulist = copy.Vertices.Intersect(foo).ToList();
                if (ulist.Count != 1)
                    throw new Exception("Error copying tree");
                var u = ulist[0];
                foo.Clear();
                foo.Add(e.Target);
                var vlist = copy.Vertices.Intersect(foo).ToList();
                if (vlist.Count != 1)
                    throw new Exception("Error copying tree");
                var v = vlist[0];
                copy.ConnectChild(u, v);             
            }
            return copy;
        }

        /// <summary>
        /// Constructs a new tree from the root of the subtree of an existing tree
        /// </summary>
        /// <param name="root">The root of the new tree</param>
        /// <returns>A new Tree object containing all the descendants of the root</returns>
        public static Tree fromTreeVertex(TreeVertex root)
        {
            var T = new Tree();
            var Q = new Queue<TreeVertex>();
            Q.Enqueue(root);
            while (Q.Count != 0)
            {
                var v = Q.Dequeue();
                var nv = new TreeVertex();
                nv.bijectedVertices = new BitArray(v.bijectedVertices);
                
                if (v != root)
                {
                    var par = T.findVertex(v.parent.bijectedVertices);               
                    T.ConnectChild(par, nv);
                }
                T.Vertices.Add(nv);
                foreach (TreeVertex c in v.children)
                    Q.Enqueue(c);
            }
            return T;
        }
        
        /// <summary>
        /// Applies the chosen neighborhood operator to a tree
        /// </summary>
        /// <param name="g">The graph we are decomposing</param>
        /// <param name="t">The tree we want to get a neighbor from</param>
        /// <param name="op">The neighborhood operator</param>
        /// <returns>A neighbor solution of t</returns>
        public static Tree getNeighbor(Tree t, NeighborhoodOperator op)
        {
            switch (op)
            {
                case NeighborhoodOperator.uncleSwap:
                    return _uncleSwap(t);
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

        /// <summary>
        /// Helper method of getNeighbor, applies the twoswap neighborhood operator
        /// </summary>
        /// <param name="t">The tree we want a neighbor from</param>
        /// <returns>A neighbor solution of t</returns>
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
            var check = neighbor.Edges.Count;
            neighbor.DisconnectChild(vapar, va);
            neighbor.DisconnectChild(vbpar, vb);
            if (!(check == neighbor.Edges.Count + 2))
                throw new Exception();
            neighbor.ConnectChild(vapar, vb);
            neighbor.ConnectChild(vbpar, va);
                           
            var ancest = vapar;
            while (ancest != null)
            {
                ancest.bijectedVertices = ancest.bijectedVertices.And(new BitArray(va.bijectedVertices).Not());
                ancest = ancest.parent;
            }
            var bancest = vbpar;
            while (bancest != null)
            {
                bancest.bijectedVertices = bancest.bijectedVertices.And(new BitArray(vb.bijectedVertices).Not()).Or(va.bijectedVertices);
                bancest = bancest.parent;
            }
            var cancest = vapar;
            while (cancest != null)
            {
                cancest.bijectedVertices = cancest.bijectedVertices.Or(vb.bijectedVertices);
                cancest = cancest.parent;
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
            Tree bestSolution = null;
            bool improvedsolution = false;
            int originalMMw = Algorithms.Hopcroft_Karp.GetFitness(g, t);
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
                    var dadMatchCount = Program.HK.GetMMSize(g, dad.bijectedVertices);
                    //If the swap can reduce MM-width, or if there is no good solution yet, check if the swap makes an improvement
                        var duncleverts = new BitArray(dad.bijectedVertices).And(new BitArray(v.bijectedVertices).Not()).Or(uncle.bijectedVertices);
                        var duncleMatchCount = Program.HK.GetMMSize(g,duncleverts);
                        //Check if the swap would improve this part of the tree
                        if (duncleMatchCount < dadMatchCount || !improvedsolution)
                        {
                            var neighbor = copyExisting(t);
                            var neighvert = neighbor.findVertex(v.bijectedVertices);
                            _swapUncle(neighbor, neighvert);
                            int neighborMMw = Algorithms.Hopcroft_Karp.GetFitness(g, neighbor);
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
            dad.bijectedVertices = dad.bijectedVertices.And(new BitArray(v.bijectedVertices).Not()).Or(uncle.bijectedVertices);
        }

        /// <summary>
        /// Improves the solution by continually taking the best neighbor until this is no longer an improvement
        /// </summary>
        /// <param name="g">the graph we are decomposing</param>
        /// <param name="T">Our initial solution</param>
        /// <param name="op">The neighborhood operator</param>
        /// <param name="msToRun">After how many milliseconds we should time out and return the best solution found so far</param>
        /// <returns>A solution that is a local optimum with respect to the neighborhood operator</returns>
        public static Tree ConvergingIteratedLocalSearch(Graph g, Tree T, NeighborhoodOperator op, double msToRun)
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
