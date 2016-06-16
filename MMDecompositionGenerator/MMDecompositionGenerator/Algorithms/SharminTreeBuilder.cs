//SharminTreeBuilder.cs
//Builds tree decompositions using methods based on those used in "Practical Aspects of the Graph Parameter Boolean-Width" by dr. Sadia Sharmin (http://bora.uib.no/bitstream/handle/1956/8406/dr-thesis-2014-Sadia-Sharmin.pdf?sequence=1)

using System;
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
        private Dictionary<List<Vertex>,Tree> Best;
        private List<Vertex> allvertices;
        private IMatchingAlgorithm alg;
        public bool keepbalanced;

        /// <summary>
        /// Constructor for the object
        /// </summary>
        /// <param name="alg">The algorithm to be used when determining matchings</param>
        /// <param name="keepbalanced">Bool indicating if the tree should be balanced (balanced here meaning that each child of a vertex has at least 1/3rd of its bijected vertices). Must be set to false if improving a tree made with a non-sharmin heuristic</param>
        public SharminTreeBuilder(IMatchingAlgorithm alg, bool keepbalanced)
        {
            Best = new Dictionary<List<Vertex>, Tree>(new PartComparer());
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
            root.bijectedVertices.AddRange(g.vertices.Values.ToList());
            decomposition.Vertices.Add(root);
            //Split the top vertices into partitions until a full decomposition is made.
            S.Push(root);
            while (S.Count > 0)
            {
                var v = S.Pop();
                if (v.bijectedVertices.Count > 1)
                {
                    var a = _Split(g, v);
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
        /// Helper method of _TryToImproveSubtree, randomly exchanges some bijected vertices of two tree vertices
        /// </summary>
        /// <param name="x">The first treevertex to have its bijected vertices swapped</param>
        /// <param name="y">The second treevertex to have its bijected vertices swapped</param>
        /// <returns>The first part of the new partition of the combined bijected vertices made</returns>
        private List<Vertex> _RandomSwap(TreeVertex y, TreeVertex z)
        {
            var A = new List<Vertex>();
            var Py = new List<Vertex>();
            Py.AddRange(y.bijectedVertices);
            var Pz = new List<Vertex>();
            Pz.AddRange(z.bijectedVertices);
            var rand = new Random();
            int i, j;
            if (keepbalanced)
            {
                i = rand.Next(Py.Count - (y.parent.bijectedVertices.Count / 3));
                j = rand.Next(Pz.Count - (z.parent.bijectedVertices.Count / 3));
            }
            else
            {
                i = rand.Next(Py.Count);
                j = rand.Next(Pz.Count);
            }
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

            allvertices = g.vertices.Values.ToList();
            allvertices.Sort();
                     
            foreach (TreeVertex v in T.Vertices)
            {
                v.bijectedVertices.Sort();
                if (!Best.ContainsKey(v.bijectedVertices))
                    Best[v.bijectedVertices] = TreeBuilder.fromTreeVertex(v);
            }

            int iterations = 0;
            var best = Hopcroft_Karp.GetMMWidth(g,Best[allvertices]);
            while (DateTime.Now < endtime)
            {
                var root = T.Root;
                _TryToImproveSubtree(g, T, root);
                iterations++;
                var neww = Hopcroft_Karp.GetMMWidth(g, Best[allvertices]);
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
            r.bijectedVertices.Sort();
            if (r.bijectedVertices.Count <= 1)
                throw new Exception("Only subtrees with more than 1 bijected vertex can be improved");
            List<Vertex> A, B;
            if (r.children.Count == 0)
                A = _Split(g, r);
            else
            {
                if (r.children.Count != 2)
                    throw new Exception("Tree is not binary");
                A = _RandomSwap(r.children[0], r.children[1]);
            }
            B = r.bijectedVertices.Except(A).ToList();
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

                A.Sort();
                B.Sort();
                if (Best.ContainsKey(A) && Hopcroft_Karp.GetMMWidth(g, Best[A]) < Hopcroft_Karp.GetMMWidth(g, Best[allvertices]))
                    t.AppendSubtree(TreeBuilder.copyExisting(Best[A]));
                else if (A.Count > 1)
                    _TryToImproveSubtree(g, t, va);
                if (Best.ContainsKey(B) && Hopcroft_Karp.GetMMWidth(g, Best[B]) < Hopcroft_Karp.GetMMWidth(g, Best[allvertices]))
                    t.AppendSubtree(TreeBuilder.copyExisting(Best[B]));
                else if (B.Count > 1)
                    _TryToImproveSubtree(g, t, vb);

                bool fullsubtree = true;
                foreach (TreeVertex tv in r.Descendants)
                {
                    if ((tv.bijectedVertices.Count > 1 && tv.children.Count != 2) || (tv.children.Count != 0 && tv.children.Count != 2))
                        fullsubtree = false;
                }
                if (fullsubtree)
                    Best[r.bijectedVertices] = TreeBuilder.fromTreeVertex(r);
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
        private List<Vertex> _Split(Graph g, TreeVertex splitvert)
        {
            int bestmmw = int.MaxValue;
            var bestpart = new List<Vertex>();
            var part = new List<Vertex>();
            var r = new Random();
            var leftoververtices = new List<Vertex>();
            leftoververtices.AddRange(splitvert.bijectedVertices);
            //Unless the splitvertex is the root, we add 1/2 of the bijected vertices to the partition at random
            if (splitvert.bijectedVertices.Count != g.vertices.Count)
            {
                
                while (part.Count < Math.Ceiling(((float)splitvert.bijectedVertices.Count) / 2))
                {
                    int rn = r.Next(leftoververtices.Count);
                    part.Add(leftoververtices[rn]);
                    leftoververtices.RemoveAt(rn);
                    if (part.Count >= Math.Ceiling(((float)splitvert.bijectedVertices.Count) / 3) && (leftoververtices.Count) >= Math.Ceiling(((float)splitvert.bijectedVertices.Count) / 3))
                    {
                        var foo = Math.Max(alg.GetMMSize(g,part), alg.GetMMSize(g,leftoververtices));
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
                    part.Add(v);
                    leftoververtices.Remove(v);
                    var foo = Math.Max(alg.GetMMSize(g,part), alg.GetMMSize(g,leftoververtices));
                    if (foo < mmw)
                    {
                        mmw = foo;
                        topv = v;
                    }
                    part.Remove(v);
                    leftoververtices.Add(v);
                }
                if (topv == null)
                    throw new Exception("Error Splitting tree vertex");

                part.Add(topv);
                leftoververtices.Remove(topv);

                if (mmw < bestmmw && part.Count >= Math.Ceiling(((float)splitvert.bijectedVertices.Count) / 3) && leftoververtices.Count >= Math.Ceiling(((float)splitvert.bijectedVertices.Count) / 3))
                {
                    bestmmw = mmw;
                    bestpart = new List<Vertex>().Union(part).ToList();
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
