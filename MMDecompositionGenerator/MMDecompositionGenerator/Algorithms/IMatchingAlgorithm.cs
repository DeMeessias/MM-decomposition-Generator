//IMatchingAlgorithm.cs
//Defines the MatchingAlgorithm interface, for algorithms capable of finding a matching in a bipartite graph
using System.Collections.Generic;
using System.Collections;

namespace MMDecompositionGenerator.Algorithms
{
    /// <summary>
    /// Interface for algorithms capable of finding a matching in a bipartite graph
    /// </summary>
    interface IMatchingAlgorithm
    {
        /// <summary>
        /// Property returning the name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The method that finds a matching
        /// </summary>
        /// <param name="g">The graph we want to find a matching of</param>
        /// <returns>A matching of the edges in the graph</returns>
        List<Data_Structures.Edge> GetMatching(Data_Structures.BipartiteGraph g);

        /// <summary>
        /// Method giving the size of a matching that a partition in a graph would give
        /// </summary>
        /// <param name="g">The graph our partition is made in</param>
        /// <param name="A">Our partition</param>
        /// <returns></returns>
        int GetMMSize(Data_Structures.Graph g, BitArray part);
    }
}
