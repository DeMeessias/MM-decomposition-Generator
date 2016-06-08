//IOptimizer.cs
//Defines the optimizer interface for algorithms that can optimize a tree
using MMDecompositionGenerator.Data_Structures;

namespace MMDecompositionGenerator.Algorithms
{
    /// <summary>
    /// Interface for algorithms that use a metaheuristic to improve a tree decomposition
    /// </summary>
    interface IOptimizer
    {
        /// <summary>
        /// The optimizing method
        /// </summary>
        /// <param name="g">The graph we are decomposing</param>
        /// <param name="T">Our initial solution</param>
        /// <param name="alg">The algorihm to use when getting matchings</param>
        /// <returns>A better tree decomposition</returns>
    Tree Optimize(Graph g, Tree T, double msToRun);
    }
}
