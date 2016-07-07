//SimulatedAnnealing.cs
//Implements the simulated annealing metaheuristic

using System;
using System.Collections.Generic;
using MMDecompositionGenerator.Data_Structures;

namespace MMDecompositionGenerator.Algorithms
{
    /// <summary>
    /// Optimizer using Simulated Annealing to improve a tree decomposition
    /// </summary>
    class SimulatedAnnealing : IOptimizer
    {
        TreeBuilder.NeighborhoodOperator op;
        double startTemperature;
        int decreaseIterations;
        float tempMultiplier;

        /// <summary>
        /// Property returning the name
        /// </summary>
        public string Name {
        get {
                switch (op)
                {
                    case TreeBuilder.NeighborhoodOperator.uncleSwap:
                        return "SAuncle";
                    case TreeBuilder.NeighborhoodOperator.twoswap:
                        return "SA2swap";
                    default:
                        throw new NotImplementedException();
                }
            } }

        /// <summary>
        /// Constructor for the instance of the Simulated Annealing algorithm
        /// </summary>
        /// <param name="op">The neighborhood operator</param>
        /// <param name="startTemperature">The starting temperature</param>
        /// <param name="decreaseIterations">How many times we should iterate before decreasing the temperature</param>
        /// <param name="tempMultiplier">Multiplier that determines how fast the temperature decreases, should be between 0 and 1 (exclusive), usually close to 1</param>
        public SimulatedAnnealing(TreeBuilder.NeighborhoodOperator op, double startTemperature, int decreaseIterations, float tempMultiplier)
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
            double temperature = startTemperature;
            var currentSolution = T;
            var bestSolution = T;
            int currFitness = Hopcroft_Karp.GetFitness(g, T);
            int bestFitness = currFitness;
            var rand = new Random();
            int iterations = 0;
            var difs = new List<int>();
            while (DateTime.Now < endtime)
            {
                var neighbor = TreeBuilder.getNeighbor(currentSolution, op);
                int nFitness = Hopcroft_Karp.GetFitness(g, neighbor);
                if (nFitness < bestFitness)
                {
                    bestFitness = nFitness;
                    bestSolution = neighbor;
                    Program.WriteToLog("BS " + bestFitness + " in " + (DateTime.Now - starttime).TotalMilliseconds + " ms (i " + iterations + ")");
                }

                if (nFitness < currFitness)
                {
                    currentSolution = neighbor;
                    currFitness = nFitness;
                }
                else
                {
                    double p = Math.Exp((currFitness - nFitness) / temperature);
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
                    if (temperature <= 2.17)
                    {
                        temperature = startTemperature;
                        currentSolution = bestSolution;
                    }
                }
            }
            Console.WriteLine(iterations);
            return bestSolution;
        }
    }
}
