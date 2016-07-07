//BottomUp.cs
//Implements a number of bottom up construction heuristics
using System;
using System.Collections;
using System.Collections.Generic;
using MMDecompositionGenerator.Data_Structures;

namespace MMDecompositionGenerator.Algorithms
{
    class BottomUp : IConstructor
    {
        //Possible construction heuristics for building trees
        public enum ConstructionHeuristic { bAllpairs, bRandomGreedy, bSmallest, bcompletelyRandom, lbRandom, lbGreedy }
        ConstructionHeuristic h;
        IMatchingAlgorithm alg;

        public string Name { get { string name = "BU";
                switch (h)
                {
                    case ConstructionHeuristic.bAllpairs:
                        name += "allPairsGreedy";
                        break;
                    case ConstructionHeuristic.bcompletelyRandom:
                        name += "completelyRandom";
                        break;
                    case ConstructionHeuristic.bSmallest:
                        name += "smallestGreedy";
                        break;
                    case ConstructionHeuristic.bRandomGreedy:
                        name += "randomGreedy";
                        break;
                }
                if (alg is Hopcroft_Karp)
                    name += "HK";
                else if (alg is fastMaximal)
                    name += "fastMax";
                return name;
            } }

        /// <summary>
        /// Constructor for the BottomUp object
        /// </summary>
        /// <param name="h">The construction heuristic we want to use</param>
        /// <param name="alg">The algorithm we want to use to generate matchings</param>
        public BottomUp(ConstructionHeuristic h, IMatchingAlgorithm alg)
        {
            this.h = h;
            this.alg = alg;
        }
        /// <summary>
        /// Constructs a new tree decomposition of a graph heuristically
        /// </summary>
        /// <param name="g">The graph we are decomposing</param>
        /// <returns>A tree decomposition of the graph</returns>
        public Tree Construct(Graph g)
        {
            return _fromGraphBottomUp(g, h, alg);
        }
                   /// <summary>
                   /// Builds a tree composition of a graph bottom-up using a construction heuristic.
                   /// </summary>
                   /// <param name="g">The graph we want to decompose</param>
                   /// <param name="h">The heuristic we use to build the tree</param>
                   /// <returns>A tree decomposition of g</returns>
        private static Tree _fromGraphBottomUp(Graph g, ConstructionHeuristic h, IMatchingAlgorithm alg)
        {
            //var alg = new Algorithms.fastMaximal();
            var topVertices = new List<TreeVertex>();
            var decomposition = new Tree();
            //Add all the graph vertices as leaves in the tree
            foreach (Vertex v in g.vertices.Values)
            {
                var tv = new TreeVertex();
                tv.bijectedVertices[v.BitIndex] = true;
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
                    case ConstructionHeuristic.bRandomGreedy:
                        var r = new Random();
                        int rn = r.Next(topVertices.Count);
                        mv = topVertices[rn];
                        ov = null;
                        mmw = int.MaxValue;
                        for (int i = 0; i < topVertices.Count; i++)
                        {
                            if (i != rn)
                            {
                                var bivertices = new BitArray(mv.bijectedVertices);                               
                                bivertices = bivertices.Or(topVertices[i].bijectedVertices);
                                var mw = alg.GetMMSize(g,bivertices);

                                if (mw < mmw)
                                {
                                    ov = topVertices[i];
                                    mmw = mw;
                                }
                            }
                        }

                        break;
                    case ConstructionHeuristic.bSmallest:
                        mv = topVertices[0];
                        
                        var mvcount = int.MaxValue;
                        foreach (TreeVertex topv in topVertices)
                        {
                            var topvcount = 0;
                            for (int i = 0; i < topv.bijectedVertices.Count; i++)
                                if (topv.bijectedVertices[i])
                                    topvcount++;
                            if (topvcount < mvcount)
                            {
                                mv = topv;
                                mvcount = topvcount;
                            }
                        }
                        mmw = int.MaxValue;
                        for (int i = 0; i < topVertices.Count; i++)
                        {
                            if (topVertices[i] != mv)
                            {
                                var bivertices = new BitArray(mv.bijectedVertices);
                                bivertices = bivertices.Or(topVertices[i].bijectedVertices);
                                int mw = alg.GetMMSize(g,bivertices);
                                if (mw < mmw)
                                {
                                    ov = topVertices[i];
                                    mmw = mw;
                                }
                            }
                        }
                        break;
                    case ConstructionHeuristic.bAllpairs:
                        mmw = int.MaxValue;
                        for (int i = 0; i < topVertices.Count - 1; i++)
                            for (int j = 1; j < topVertices.Count; j++)
                            {
                                if (i < j)
                                {
                                    var bivertices = new BitArray(topVertices[i].bijectedVertices);
                                    bivertices = bivertices.Or(topVertices[j].bijectedVertices);
                                    int mw = alg.GetMMSize(g, bivertices);
                                    if (mw < mmw)
                                    {
                                        mv = topVertices[i];
                                        ov = topVertices[j];
                                        mmw = mw;
                                    }
                                }
                            }
                        break;
                    case ConstructionHeuristic.bcompletelyRandom:
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
                nv.bijectedVertices = new BitArray(mv.bijectedVertices).Or(ov.bijectedVertices);
            }
            return decomposition;
        }
    }
    
}
