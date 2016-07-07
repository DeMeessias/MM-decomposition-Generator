//SharminTreeBuilder.cs
//Builds tree decompositions using methods based on those used in "Practical Aspects of the Graph Parameter Boolean-Width" by dr. Sadia Sharmin (http://bora.uib.no/bitstream/handle/1956/8406/dr-thesis-2014-Sadia-Sharmin.pdf?sequence=1)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MMDecompositionGenerator.Data_Structures;

namespace MMDecompositionGenerator.Algorithms
{
    /// <summary>
    /// Class that holds methods for building tree decompositions using Sharmin-inspired methods
    /// </summary>
    class SharminTreeBuilder : IConstructor, IOptimizer
    {
        public string Name
        {
            get { string name = "";
                name += "Sharmin";
                name += alg.Name;
                if (keepbalanced)
                    name += "b";
                else name += "ub";
                return name;
            }
        }
        private Dictionary<BitArray,Tree> Best;
        private BitArray allvertices;
        private IMatchingAlgorithm alg;
        public bool keepbalanced;

        /// <summary>
        /// Constructor for the object
        /// </summary>
        /// <param name="alg">The algorithm to be used when determining matchings</param>
        /// <param name="keepbalanced">Bool indicating if the tree should be balanced (balanced here meaning that each child of a vertex has at least 1/3rd of its bijected vertices). Must be set to false if improving a tree made with a non-sharmin heuristic</param>
        public SharminTreeBuilder(IMatchingAlgorithm alg, bool keepbalanced)
        {
            Best = new Dictionary<BitArray, Tree>(new PartComparer());
            allvertices = null;
            this.alg = alg;
            this.keepbalanced = keepbalanced;
        }

        /// <summary>
        /// Builds a tree decomposition of a graph top down using Sharmin's construction heuristic
        /// </summary>
        /// <param name="g">The graph we want to decompose</param>
        /// <param name="h">The construction heuristic we want to use</param>
        /// <returns>A Tree decomposition of g</returns>
        public Tree Construct(Graph g)
        {
            var decomposition = new Tree();
            var root = new TreeVertex();
            var S = new Stack<TreeVertex>();
            //Create a root vertex that has a bijection to all of the vertices of g
            foreach (Vertex v in g.vertices.Values)
                root.bijectedVertices[v.BitIndex] = true;
            decomposition.Vertices.Add(root);
            //Split the top vertices into partitions until a full decomposition is made.
            S.Push(root);
            while (S.Count > 0)
            {
                var v = S.Pop();
                int vbijcount = 0;
                for (int i = 0; i < v.bijectedVertices.Count; i++)
                    if (v.bijectedVertices[i])
                        vbijcount++;
                if (vbijcount > 1)
                {
                    var a = _Split(g, v);
                    var b = new BitArray(v.bijectedVertices).And(new BitArray(a).Not());
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
                else if (vbijcount != 1)
                    throw new Exception("Error constructing tree");
            }
            int vertcheck = 2 * g.vertices.Count - 1;
            if (decomposition.Vertices.Count != vertcheck)
                throw new Exception("Error constructing tree");
            return decomposition;
        }

        /// <summary>
        /// Helper method of _TryToImproveSubtree, randomly exchanges some bijected vertices of two tree vertices
        /// </summary>
        /// <param name="x">The first treevertex to have its bijected vertices swapped</param>
        /// <param name="y">The second treevertex to have its bijected vertices swapped</param>
        /// <returns>The first part of the new partition of the combined bijected vertices made</returns>
        private BitArray _RandomSwap(TreeVertex y, TreeVertex z)
        {
            BitArray A;
            var Py = new List<int>();
            var Pz = new List<int>();
            for (int c = 0; c < y.bijectedVertices.Count; c++)
            {
                if (y.bijectedVertices[c])
                    Py.Add(c);
            }
            for (int c = 0; c < z.bijectedVertices.Count; c++)
            {
                if (z.bijectedVertices[c])
                    Pz.Add(c);
            }
            int parcount = Py.Count + Pz.Count;
            var rand = new Random();
            int i, j;
            if (keepbalanced)
            {
                i = rand.Next(Py.Count - (parcount / 3));
                j = rand.Next(Pz.Count - (parcount / 3));
            }
            else
            {
                i = rand.Next(Py.Count);
                j = rand.Next(Pz.Count);
            }
            var Mi = new BitArray(Program.numverts);
            var Mj = new BitArray(Program.numverts);
            for (int k = 0; k < i; k++)
            {
                int rn = rand.Next(Py.Count);
                Mi[Py[rn]] = true;
                Py.RemoveAt(rn);
            }
            for (int k = 0; k < j; k++)
            {
                int rn = rand.Next(Pz.Count);
                Mj[Pz[rn]] = true;
                Pz.RemoveAt(rn);
            }
            A = new BitArray(y.bijectedVertices).And(Mi.Not()).Or(Mj);
            return A;
        }

        /// <summary>
        /// Uses local search to try and improve the solution
        /// </summary>
        /// <param name="g">The graph we are decomposing</param>
        /// <param name="T">The initial solution</param>
        /// <param name="msToRun">How many milliseconds we should continue the search</param>
        /// <returns></returns>
        public Tree Optimize(Graph g, Tree T, double msToRun)
        {
            var starttime = DateTime.Now;
            var endtime = starttime.AddMilliseconds(msToRun);

            allvertices = new BitArray(Program.numverts);
            foreach (Vertex v in g.vertices.Values)
                allvertices[v.BitIndex] = true;
                                    
            foreach (TreeVertex v in T.Vertices)
            {
                if (!Best.ContainsKey(v.bijectedVertices))
                    Best[new BitArray(v.bijectedVertices)] = TreeBuilder.fromTreeVertex(v);
            }

            int iterations = 0;
            var best = Hopcroft_Karp.GetFitness(g,Best[allvertices]);
            while (DateTime.Now < endtime)
            {
                var root = T.Root;
                _TryToImproveSubtree(g, T, root);
                iterations++;
                var neww = Hopcroft_Karp.GetFitness(g, Best[allvertices]);
                if (neww < best)
                {
                    best = neww;
                    Program.WriteToLog("BS " + neww + " in " + (DateTime.Now - starttime).TotalMilliseconds + " ms (i " + iterations + ")");                       
                }
            }
            return Best[allvertices];
        }

        /// <summary>
        /// Helper method of Optimize. Recursively tries to improve a subtree of the tree decomposition
        /// </summary>
        /// <param name="g">The graph we are decomposing</param>
        /// <param name="t">The tree decomposition</param>
        /// <param name="r">The root of the subtree we want to improve</param>
        public void _TryToImproveSubtree(Graph g, Tree t, TreeVertex r)
        {
            int check = 0;
            for (int i = 0; i < Program.numverts; i++)
                if (r.bijectedVertices[i])
                    check++;
            if (check <= 1)
                throw new Exception("Only subtrees with more than 1 bijected vertex can be improved");
            BitArray A, B;
            if (r.children.Count == 0)
                A = _Split(g, r);
            else
            {
                if (r.children.Count != 2)
                    throw new Exception("Tree is not binary");
                A = _RandomSwap(r.children[0], r.children[1]);
            }
            B = new BitArray(r.bijectedVertices).And(new BitArray(A).Not());
            if (Math.Max(alg.GetMMSize(g, A), alg.GetMMSize(g, B)) < Hopcroft_Karp.GetMMWidth(g,Best[allvertices]))
            {
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
                var va = new TreeVertex();
                va.bijectedVertices = A;
                t.Vertices.Add(va);
                t.ConnectChild(r, va);
                var vb = new TreeVertex();
                vb.bijectedVertices = B;
                t.Vertices.Add(vb);
                t.ConnectChild(r, vb);

                if (Best.ContainsKey(A) && Hopcroft_Karp.GetFitness(g, Best[A]) < Hopcroft_Karp.GetFitness(g, Best[allvertices]))
                    t.AppendSubtree(TreeBuilder.copyExisting(Best[A]));
                else {
                    var acount = 0;
                    for (int i = 0; i < A.Count; i++)
                        if (A[i])
                            acount++;
                    if (acount > 1)
                        _TryToImproveSubtree(g, t, va);
                     }
                if (Best.ContainsKey(B) && Hopcroft_Karp.GetFitness(g, Best[B]) < Hopcroft_Karp.GetFitness(g, Best[allvertices]))
                    t.AppendSubtree(TreeBuilder.copyExisting(Best[B]));
                else
                {
                    var bcount = 0;
                    for (int i = 0; i < B.Count; i++)
                        if (B[i])
                            bcount++;
                    if (bcount > 1)
                        _TryToImproveSubtree(g, t, vb);
                }
                bool fullsubtree = true;
                foreach (TreeVertex tv in r.Descendants)
                {
                    var tvbijcount = 0;
                    for (int i = 0; i < tv.bijectedVertices.Count; i++)
                        if (tv.bijectedVertices[i])
                            tvbijcount++;
                    if ((tvbijcount > 1 && tv.children.Count != 2) || (tv.children.Count != 0 && tv.children.Count != 2))
                        fullsubtree = false;
                }
                if (fullsubtree)
                    Best[new BitArray(r.bijectedVertices)] = TreeBuilder.fromTreeVertex(r);
            }
            else if (r.children.Count != 0)
                return;

        }

        /// <summary>
        /// Helper method for Construct. Splits the set of bijected vertices of a tree vertex into two parts to be turned into a subtree
        /// </summary>
        /// <param name="g">The graph the tree decomposition is based on</param>
        /// <param name="splitvert">The tree vertex to split</param>
        /// <param name="alg">The algorithm used to determine a maximum matching</param>
        /// <returns>A list of bijected vertices in the first part of the best partition found</returns>
        private BitArray _Split(Graph g, TreeVertex splitvert)
        {
            int bestmmw = int.MaxValue;
            var bestpart = new BitArray(Program.numverts);
            var part = new BitArray(Program.numverts);
            var r = new Random();
            var leftoververtices = new BitArray(splitvert.bijectedVertices);
            bool root = true;
            var leftoverindices = new List<int>();
            for (int i = 0; i < leftoververtices.Count; i++)
            {
                if (!leftoververtices[i])
                    root = false;
                else leftoverindices.Add(i);
            }
            var partcount = 0;
            int totalsize = leftoverindices.Count;

            //Unless the splitvertex is the root, we add 1/2 of the bijected vertices to the partition at random
            if (!root)
            {
                
                while (partcount < Math.Ceiling((float)(totalsize / 2)))
                {
                    int rn = r.Next(leftoverindices.Count);
                    part[leftoverindices[rn]] = true;
                    partcount++;
                    leftoververtices[leftoverindices[rn]] = false;
                    leftoverindices.RemoveAt(rn);
                    if (partcount >= Math.Ceiling(((float)totalsize) / 3) && (leftoverindices.Count) >= Math.Ceiling(((float)totalsize) / 3))
                    {
                        var foo = Math.Max(alg.GetMMSize(g,part), alg.GetMMSize(g,leftoververtices));
                        if (foo < bestmmw)
                        {
                            bestmmw = foo;
                            bestpart = new BitArray(part);
                        }
                    }
                }
            }
            //Create possible partitions until the remainder of the bijected vertices becomes too small.
            while (totalsize - partcount > Math.Ceiling(((float)totalsize) / 3))
            {                
                int mmw = int.MaxValue;
                int topv = -1;
                foreach (int v in leftoverindices)
                {
                    part[v] = true;
                    leftoververtices[v] = false;
                    var foo = Math.Max(alg.GetMMSize(g,part), alg.GetMMSize(g,leftoververtices));
                    if (foo < mmw)
                    {
                        mmw = foo;
                        topv = v;
                    }
                    part[v] = false;
                    leftoververtices[v] = true;
                }
                if (topv == -1)
                    throw new Exception("Error Splitting tree vertex");

                part[topv] = true;
                partcount++;
                leftoververtices[topv] = false;
                leftoverindices.Remove(topv);

                if (mmw < bestmmw && partcount >= Math.Ceiling(((float)totalsize) / 3) && leftoverindices.Count >= Math.Ceiling(((float)totalsize) / 3))
                {
                    bestmmw = mmw;
                    bestpart = new BitArray(part);
                }
            }

            //Return the best partition found among the created partitions
            return bestpart;
        }

        /// <summary>
        /// Clears the cache
        /// </summary>
        public void ClearCache()
        {
            Best.Clear();
        }
    }
}
