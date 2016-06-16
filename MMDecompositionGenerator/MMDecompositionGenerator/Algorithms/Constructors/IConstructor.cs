//IConstructor.cs
//Interface for algorithms that construct a tree decomposition
using MMDecompositionGenerator.Data_Structures;

namespace MMDecompositionGenerator.Algorithms
{
    /// <summary>
    /// Iterface for algorithms that heuristically generate an initial tree decomposition
    /// </summary>
    interface IConstructor
    {
        /// <summary>
        /// Property returning the name
        /// </summary>
        string Name { get;}

        /// <summary>
        /// Method for constructing a tree decomposition from the graph
        /// </summary>
        /// <param name="g">The graph we are decomposing</param>
        /// <returns>A tree decomposition of g</returns>
        Tree Construct(Graph g);
    }
}
