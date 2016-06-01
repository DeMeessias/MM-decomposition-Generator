//MatchingAlgorithm.cs
//Defines the MatchingAlgorithm interface, for algorithms capable of finding a matching in a bipartite graph
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMDecompositionGenerator.Algorithms
{
    /// <summary>
    /// Interface for algorithms capable of finding a matching in a bipartite graph
    /// </summary>
    interface IMatchingAlgorithm
    {
        /// <summary>
        /// The method that finds a matching
        /// </summary>
        /// <param name="g">The graph we want to find a matching of</param>
        /// <returns>A matching of the edges in the graph</returns>
        List<Data_Structures.Edge> GetMatching(Data_Structures.BipartiteGraph g);
    }
}
