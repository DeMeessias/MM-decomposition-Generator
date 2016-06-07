//Hopcroft-Karp.cs
//Implements the Hopcroft-Karp algorithm (http://www.cs.princeton.edu/courses/archive/spr10/cos423/handouts/algorithmformaximum.pdf) for getting a maximum matching in a bipartite graph

using System;
using System.Collections.Generic;
using System.Linq;
using MMDecompositionGenerator.Data_Structures;

namespace MMDecompositionGenerator.Algorithms
{
    /// <summary>
    /// Class containing methods utilising the Hopcroft-Karp algorithm
    /// </summary>
    class Hopcroft_Karp : IMatchingAlgorithm
    {
        private Dictionary<List<Vertex>, int> cache;

        public Hopcroft_Karp()
        {
            if (Program.HK != null)
                throw new Exception("Only 1 instance of Hopcroft_Karp allowed. Use Program.HK instead of making a new instance");
            cache = new Dictionary<List<Vertex>, int>(new PartComparer());
        }

        /// <summary>
        /// Gets a (maximum) matching from a bipartite graph
        /// </summary>
        /// <param name="g">The biparite graph we want to get a matching for</param>
        /// <returns>A list of edges that form a maximum matching</returns>
        public List<Edge> GetMatching(BipartiteGraph g)
        {
            //Initialize an empty matching
            var M = new List<Data_Structures.Edge>();

            //Keep iterating until no more M-augmenting paths can be found
            while (true)
            {
                //Find M-augmenting paths
                var SAP = _findShortestAugmentingPaths(M,g);
                if (SAP.Count == 0)
                {
                    return M;
                }

                //Take the symmetric difference between M and the shortest augmenting paths. This increases the size of our matching
                foreach (List<Data_Structures.Edge> p in SAP)
                {
                    var mminp = M.Except(p).ToList();
                    var pminm = p.Except(M).ToList();
                    M = mminp.Union(pminm).ToList();
                }
            }
        }

        /// <summary>
        /// Helper method for finding Shortest Augmenting Paths, for use in the GetMatching method
        /// </summary>
        /// <param name="M">The matching we have so far</param>
        /// <param name="g">The bipartite graph for which we are getting a matching</param>
        /// <returns>A list of paths</returns>
        private List<List<Edge>> _findShortestAugmentingPaths(List<Edge> M, BipartiteGraph g)
        {
            var SAP = new List<List<Data_Structures.Edge>>(); //List containing the shortest augmenting paths
            var L0 = new List<Data_Structures.Vertex>().Union(g.A).ToList(); // A list containing all free vertices in A
            var FG = new List<Data_Structures.Vertex>().Union(g.B).ToList(); // "Free Girls", A list containing all free vertices in B
            
            //Fix the direction of each edge so M-augmenting paths follow directed edges from a free vertex in A to one in B
            foreach (Data_Structures.Edge e in g.edges)
            {
                if (M.Contains(e))
                {
                    if (g.A.Contains(e.v))
                    {
                        L0.Remove(e.v);
                        FG.Remove(e.u);
                        e.Flip();
                    }
                    else
                    {
                        L0.Remove(e.u);
                        FG.Remove(e.v);
                    }
                }
                else
                    if (g.A.Contains(e.u))
                        e.Flip();
            }

            //Remove the isolated vertices
            var isolatedVertices = new List<Data_Structures.Vertex>();
            foreach (Data_Structures.Vertex v in L0)
            {
                if (v.incedentEdges.Count == 0)
                    isolatedVertices.Add(v);
            }
            L0 = L0.Except(isolatedVertices).ToList();
            isolatedVertices.Clear();
            foreach (Data_Structures.Vertex v in FG)
            {
                if (v.incedentEdges.Count == 0)
                    isolatedVertices.Add(v);
            }
            FG = FG.Except(isolatedVertices).ToList();

            //Exit if it is clear no paths can be found
            if (L0.Count == 0 || FG.Count == 0)
                return SAP;

            //Build a graph containing our SAP's
            var L = new List<List<Data_Structures.Vertex>>();
            L.Insert(0,L0);
            var Lu = new List<Data_Structures.Vertex>();
            var E = new List<List<Data_Structures.Edge>>();
            int i = 0;
            while (L[i].Intersect(FG).ToList().Count == 0)
            {
                Lu = Lu.Union(L[i]).ToList();
                var Ei = new List<Data_Structures.Edge>();
                foreach (Data_Structures.Edge e in g.edges)
                {
                    if (L[i].Contains(e.v))
                        if (!Lu.Contains(e.u))
                            Ei.Add(e);
                }
                E.Insert(i, Ei);
                var Liplus1 = new List<Data_Structures.Vertex>();
                foreach (Data_Structures.Edge e in Ei)
                { Liplus1.Add(e.u); }
                L.Insert(i+1,Liplus1);
                i++;

                //Exit if it is clear no more SAP's can be found
                if (L[i].Count == 0)
                    return SAP;
            }
            var Vd = Lu.Union(L[i].Intersect(FG)).ToList();
            var Ed = new List<Data_Structures.Edge>();
            for (int j = 0; j < i - 1; j++)
            {
                Ed = Ed.Union(E[j]).ToList();
            }
            var Eb = new List<Data_Structures.Edge>();
            foreach (Data_Structures.Edge e in g.edges)
            {
                if (L[i - 1].Contains(e.v))
                    if (FG.Contains(e.u))
                        Eb.Add(e);
            }           
            Ed = Ed.Union(Eb).ToList();
            Ed = Ed.Distinct().ToList(); //Not sure if this step is needed.
            //Add a source and sink
            var s = new Data_Structures.Vertex(-2);
            var t = new Data_Structures.Vertex(-1);
            foreach (Data_Structures.Vertex v in Vd)
            {
                if (L0.Contains(v))
                    Ed.Add(new Data_Structures.Edge(v, t));
                if (FG.Contains(v))
                    Ed.Add(new Data_Structures.Edge(s, v));
            }
            Vd.Add(s);
            Vd.Add(t);

            //Use a depth-first search to find the SAP's in our graph
            var B = new List<Data_Structures.Vertex>();
            var STACK = new Stack<Data_Structures.Vertex>();
            STACK.Push(s);
            while (STACK.Count != 0)
            {
                while (_LIST(STACK.Peek(), Ed).Count != 0)
                {
                    var FIRST = _LIST(STACK.Peek(), Ed)[0];
                    Ed.Remove(new Data_Structures.Edge(FIRST, STACK.Peek()));
                    if (!B.Contains(FIRST))
                    {
                        STACK.Push(FIRST);
                        if (STACK.Peek() != t)
                            B.Add(STACK.Peek());
                        else
                        {
                            var path = new List<Data_Structures.Edge>();
                            Data_Structures.Vertex u = null;
                            Data_Structures.Vertex v = STACK.Pop();
                            while (STACK.Count != 0)
                            {
                                u = v;
                                v = STACK.Pop();
                                if (u.Index >= 0 && v.Index >= 0)
                                    path.Add(new Data_Structures.Edge(u, v));
                            }
                            SAP.Add(path);
                            STACK.Push(s);


                        }
                    }
                }
                STACK.Pop();
            }
            if (SAP.Count != 0)
            {
                int pl = SAP[0].Count;
                foreach (List<Data_Structures.Edge> p in SAP)
                    if (p.Count != pl)
                        throw new Exception("Algorithm error (Hopcroft-Karp) : Paths don't have equal length");
            }
            return SAP;
        }

        /// <summary>
        /// Helper method that gives the children of a vertex in the special graph generated in _findShortestAugmentingPaths
        /// </summary>
        /// <param name="u">The vertex whose children we want</param>
        /// <param name="Ed">The edges of the graph</param>
        /// <returns>A list of child vertices</returns>
        private List<Vertex> _LIST(Vertex u, List<Edge> Ed)
        {
            var LIST = new List<Data_Structures.Vertex>();
            foreach (Data_Structures.Edge e in Ed)
            {
                if (e.u == u)
                    LIST.Add(e.v);
            }
            return LIST;
        }

        /// <summary>
        /// Gets the MM-width of a tree by finding the vertex that induces the partition that has the largest maximum matching
        /// </summary>
        /// <param name="g">The graph our tree is based on</param>
        /// <param name="t">The tree for which we want the MM-width</param>
        /// <returns>The MM-width of the tree</returns>
        public static int GetMMWidth(Graph g, Tree t)
        {
            int MMwidth = 0;
            //Go over each vertex and find the one that gives the biggest maximum matching
            foreach (Data_Structures.TreeVertex tv in t.Vertices)
            {
                var bigraph = Data_Structures.BipartiteGraph.FromPartition(g, tv.bijectedVertices);
                int MMw = Program.HK.GetMMSize(bigraph);
                if (MMw > MMwidth)
                    MMwidth = MMw;
            }
            return MMwidth;
        }

        /// <summary>
        /// Returns the size of a maximum matching in a bipartite graph
        /// </summary>
        /// <param name="g">The bipartite graph</param>
        /// <returns>The size a maximum matching would have</returns>
        public int GetMMSize(BipartiteGraph g)
        {
            g.A.Sort();
            //Check the cache first
            if (cache.ContainsKey(g.A))
                return cache[g.A];

            var M = GetMatching(g);
            var listcopy = new List<Vertex>(g.A);
            cache.Add(listcopy, M.Count);
            return M.Count;            
        }
    }
}
