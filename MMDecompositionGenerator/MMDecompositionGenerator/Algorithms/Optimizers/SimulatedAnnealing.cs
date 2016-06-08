//SimulatedAnnealing.cs
//Implements the simulated annealing metaheuristic

using System;
using MMDecompositionGenerator.Data_Structures;

namespace MMDecompositionGenerator.Algorithms
{
    /// <summary>
    /// Optimizer using Simulated Annealing to improve a tree decomposition
    /// </summary>
    class SimulatedAnnealing : IOptimizer
    {
        TreeBuilder.NeighborhoodOperator op;
        float startTemperature;
        int decreaseIterations;
        float tempMultiplier;

        /// <summary>
        /// Constructor for the instance of the Simulated Annealing algorithm
        /// </summary>
        /// <param name="op">The neighborhood operator</param>
        /// <param name="startTemperature">The starting temperature</param>
        /// <param name="decreaseIterations">How many times we should iterate before decreasing the temperature</param>
        /// <param name="tempMultiplier">Multiplier that determines how fast the temperature decreases, should be between 0 and 1 (exclusive), usually close to 1</param>
        public SimulatedAnnealing(TreeBuilder.NeighborhoodOperator op, float startTemperature, int decreaseIterations, float tempMultiplier)
        {
        this.op = op;
        this.startTemperature = startTemperature;
        this.decreaseIterations = decreaseIterations;
        this.tempMultiplier = tempMultiplier;
        }


        /// <summary>
        /// Uses simulated annealing to try and improve a tree decomposition
        /// </summary>
        /// <param name="g">The graph the tree is a decomposition of</param>
        /// <param name="T">Our initial solution</param>
        /// <param name="msToRun">How many milliseconds we should run SA before returning a solution</param>
        /// <returns>The best tree we have found while running SA</returns>
        public Tree Optimize(Graph g, Tree T, double msToRun)
        {
            var starttime = DateTime.Now;
            var endtime = starttime.AddMilliseconds(msToRun);
            float temperature = startTemperature;
            var currentSolution = T;
            var bestSolution = T;
            int currFitness = Hopcroft_Karp.GetFitness(g, T);
            int bestFitness = currFitness;
            var rand = new Random();
            int iterations = 0;
            while (DateTime.Now < endtime)
            {
                var neighbor = TreeBuilder.getNeighbor(currentSolution, op);
                int nFitness = Hopcroft_Karp.GetFitness(g, neighbor);
                if (nFitness < bestFitness)
                {
                    bestFitness = nFitness;
                    bestSolution = neighbor;
                }

                if (nFitness < currFitness)
                {
                    currentSolution = neighbor;
                    currFitness = nFitness;
                }
                else
                {
                    double p = Math.Exp((currFitness - nFitness) / temperature);
                    //Console.WriteLine(nFitness - currFitness);
                    //Console.WriteLine(p);
                    var rn = rand.NextDouble();
                    if (rn < p)
                    {
                        currentSolution = neighbor;
                        currFitness = nFitness;
                    }
                }
                iterations++;
                if (iterations % decreaseIterations == 0)
                {
                    temperature *= tempMultiplier;
                    if (temperature <= 1)
                        temperature = startTemperature; 
                }
            }
            return bestSolution;
        }
    }
}
