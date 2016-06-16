//IteratedLocalSearch.cs
//Optimizes tree decomposistions using ILS
using System;
using MMDecompositionGenerator.Data_Structures;

namespace MMDecompositionGenerator.Algorithms
{
    /// <summary>
    /// Optimises tree decompositions using ILS
    /// </summary>
    class IteratedLocalSearch : IOptimizer
    {
        
        TreeBuilder.NeighborhoodOperator op;
        public string Name
        {
            get { switch (op)
                {
                    case TreeBuilder.NeighborhoodOperator.uncleSwap:
                        return "ILSuncle";
                    case TreeBuilder.NeighborhoodOperator.twoswap:
                        return "ILS2swap";
                    default:
                        throw new Exception("Heuristic not implemented");
                   
                }
            } }

        /// <summary>
        /// Constructor for the ILS algorithm
        /// </summary>
        /// <param name="op">The neighborhood operator to use</param>
        public IteratedLocalSearch(TreeBuilder.NeighborhoodOperator op)
        {
            this.op = op;
        }

        /// <summary>
        /// Tries to iteratively improve the solution via LS until it has run for a specified time
        /// </summary>
        /// <param name="g">The graph we are decomposing</param>
        /// <param name="T">Our initial solution</param>
        /// <param name="msToRun">How long we want to keep searching</param>
        /// <returns>The best found solution</returns>
        public Tree Optimize(Graph g, Tree T, double msToRun)
        {
            var starttime = DateTime.Now;
            var endtime = starttime.AddMilliseconds(msToRun);
            var bestSolution = T;
            int bestFitness = Hopcroft_Karp.GetFitness(g, T);
            while (DateTime.Now < endtime)
            {
                var neighbor = TreeBuilder.getNeighbor(bestSolution, op);
                var neighborFitness = Hopcroft_Karp.GetFitness(g, neighbor);
                if (neighborFitness < bestFitness)
                {
                    bestFitness = neighborFitness;
                    bestSolution = neighbor;
                }
            }
            return bestSolution;
        }
    }
}
