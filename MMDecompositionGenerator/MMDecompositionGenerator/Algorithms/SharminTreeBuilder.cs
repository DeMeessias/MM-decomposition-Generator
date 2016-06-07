using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMDecompositionGenerator.Data_Structures;

namespace MMDecompositionGenerator.Algorithms
{
    class SharminTreeBuilder
    {
        private Dictionary<List<Vertex>,Tree> Best;

        public SharminTreeBuilder()
        {
            Best = new Dictionary<List<Vertex>, Tree>(new PartComparer());
        }

        /// <summary>
        /// Builds a tree decomposition of a graph top down using Sharmin's construction heuristic
        /// </summary>
        /// <param name="g">The graph we want to decompose</param>
        /// <param name="h">The construction heuristic we want to use</param>
        /// <returns>A Tree decomposition of g</returns>
        public Tree ConstructNewTree(Graph g, Algorithms.IMatchingAlgorithm alg)
        {
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
        /// Helper method of _TryToImproveSubtree, randomly exchanges some bijected vertices of two tree vertices
        /// </summary>
        /// <param name="x">The first treevertex to have its bijected vertices swapped</param>
        /// <param name="y">The second treevertex to have its bijected vertices swapped</param>
        /// <returns>The first part of the new partition of the combined bijected vertices made</returns>
        private List<Vertex> _RandomSwap(TreeVertex y, TreeVertex z, bool keepbalanced)
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
        public Tree Optimize(Graph g, Tree T, double msToRun, bool keepbalanced, IMatchingAlgorithm alg)
        {
            g.vertices.Sort();
            foreach (TreeVertex v in T.Vertices)
            {
                v.bijectedVertices.Sort();
                if (!Best.ContainsKey(v.bijectedVertices))
                    Best[v.bijectedVertices] = TreeBuilder.fromTreeVertex(v);
            }
            var starttime = DateTime.Now;
            var endtime = starttime.AddMilliseconds(msToRun);
            while (DateTime.Now < endtime)
            {
                var root = T.Root;
                _TryToImproveSubtree(g, T, root, keepbalanced,alg);
            }
            return Best[g.vertices];
        }

        /// <summary>
        /// Helper method of Optimize. Recursively tries to improve a subtree of the tree decomposition
        /// </summary>
        /// <param name="g">The graph we are decomposing</param>
        /// <param name="t">The tree decomposition</param>
        /// <param name="r">The root of the subtree we want to improve</param>
        public void _TryToImproveSubtree(Graph g, Tree t, TreeVertex r, bool keepbalanced, IMatchingAlgorithm alg)
        {
            r.bijectedVertices.Sort();
            if (r.bijectedVertices.Count <= 1)
                throw new Exception("Only subtrees with more than 1 bijected vertex can be improved");
            List<Vertex> A, B;
            if (r.children.Count == 0)
                A = _Split(g, r, alg);
            else
            {
                if (r.children.Count != 2)
                    throw new Exception("Tree is not binary");
                A = _RandomSwap(r.children[0], r.children[1], keepbalanced);
            }
            B = r.bijectedVertices.Except(A).ToList();
            if (Math.Max(Program.HK.GetMMSize(BipartiteGraph.FromPartition(g, A)), Program.HK.GetMMSize(BipartiteGraph.FromPartition(g, B))) < Hopcroft_Karp.GetMMWidth(g,Best[g.vertices]))
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
                if (Best.ContainsKey(A) && Hopcroft_Karp.GetMMWidth(g, Best[A]) < Hopcroft_Karp.GetMMWidth(g, Best[g.vertices]))
                    t.AppendSubtree(TreeBuilder.copyExisting(Best[A]));
                else if (A.Count > 1)
                    _TryToImproveSubtree(g, t, va, keepbalanced,alg);
                if (Best.ContainsKey(B) && Hopcroft_Karp.GetMMWidth(g, Best[B]) < Hopcroft_Karp.GetMMWidth(g, Best[g.vertices]))
                    t.AppendSubtree(TreeBuilder.copyExisting(Best[B]));
                else if (B.Count > 1)
                    _TryToImproveSubtree(g, t, vb, keepbalanced,alg);

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
        /// Helper method for _fromGraphTopDown using the tSharmin heuristic. Splits the set of bijected vertices of a tree vertex into two parts to be turned into a subtree
        /// </summary>
        /// <param name="g">The graph the tree decomposition is based on</param>
        /// <param name="splitvert">The tree vertex to split</param>
        /// <param name="alg">The algorithm used to determine a maximum matching</param>
        /// <returns>A list of bijected vertices in the first part of the best partition found</returns>
        private static List<Vertex> _Split(Graph g, TreeVertex splitvert, IMatchingAlgorithm alg)
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
                        var foo = Math.Max(alg.GetMMSize(bgraph), alg.GetMMSize(bgraph2));
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
                    var foo = Math.Max(alg.GetMMSize(bgraph), alg.GetMMSize(bgraph2));
                    if (foo < mmw)
                    {
                        mmw = foo;
                        topv = v;
                    }

                }
                if (topv == null)
                    throw new Exception("Error Splitting tree vertex");
                part.Add(topv);
                if (mmw < bestmmw && part.Count >= Math.Ceiling(((float)splitvert.bijectedVertices.Count) / 3) && (splitvert.bijectedVertices.Count - part.Count) >= Math.Ceiling(((float)splitvert.bijectedVertices.Count) / 3))
                {
                    bestmmw = mmw;
                    bestpart = new List<Vertex>().Union(part).ToList();
                }
            }

            //Return the best partition found among the created partitions
            return bestpart;
        }
    }
}
