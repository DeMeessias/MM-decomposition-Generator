//MatchingAlgorithm.cs
//Defines the MatchingAlgorithm interface, for algorithms capable of finding a matching in a bipartite graph
using System.Collections.Generic;

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

        /// <summary>
        /// Method giving the size a matching of the given graph would have
        /// </summary>
        /// <param name="g">The graph we want to know the size of</param>
        /// <returns>The size of a matching in the graph</returns>
        int GetMMSize(Data_Structures.BipartiteGraph g);
    }
}
