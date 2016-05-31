﻿//fastMaximal.cs
//Defines an algorithm for quickly getting a maximal (not necessarily maximum) matching in a Bipartite Graph

using System.Collections.Generic;

namespace MMDecompositionGenerator.Algorithms
{
    /// <summary>
    /// Class containing methods utilising the fastMaximal algorithm
    /// </summary>
    class fastMaximal
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
    }
}