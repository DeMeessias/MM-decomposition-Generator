//fastMaximal.cs
//Defines an algorithm for quickly getting a maximal (not necessarily maximum) matching in a Bipartite Graph

using System;
using System.Collections.Generic;
using MMDecompositionGenerator.Data_Structures;

namespace MMDecompositionGenerator.Algorithms
{
    /// <summary>
    /// Class containing methods utilising the fastMaximal algorithm
    /// </summary>
    class fastMaximal : IMatchingAlgorithm
    {
        /// <summary>
        /// Gets a (maximal) matching from a Bipartite Graph
        /// </summary>
        /// <param name="g">A bipartite graph for which we want to find a matching</param>
        /// <returns>A list of edges forming a maximal matching</returns>
        public List<Data_Structures.Edge> GetMatching(Data_Structures.BipartiteGraph g)
        {
            //initialize empty matching
            var M  = new List<Data_Structures.Edge> ();

            //Go over all vertices in one of the partitions. If they are unmatched, try to put them in the matching
            foreach (Data_Structures.Vertex v in g.A)
            {
                if (v.incedentEdges.Count > 0)
                {
                    bool matched = false;
                    foreach (Data_Structures.Edge e in v.incedentEdges)
                        if (M.Contains(e))
                            matched = true;
                    if (!matched)
                        M.Add(v.incedentEdges[0]);
                }
            }
            return M;
        }

        /// <summary>
        /// Returns the size of a matching
        /// </summary>
        /// <param name="g">The graph we want to know the size of a matching of</param>
        /// <returns></returns>
        public int GetMMSize(BipartiteGraph g)
        {
            return GetMatching(g).Count;
        }
    }
}
