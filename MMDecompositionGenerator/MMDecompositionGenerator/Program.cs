//Program.cs
//Gives the entry point for the application

//#define TRYCATCH //if defined, exceptions that are thrown are displayed, after which the program exits
using System;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using MMDecompositionGenerator.Data_Structures;
using MMDecompositionGenerator.Algorithms;

namespace MMDecompositionGenerator
{
    /// <summary>
    /// Class for holding the main method and defining global variables
    /// </summary>
    class Program
    {
        //Singleton instance of the Hopcroft_Karp algorithm, so everything uses the same cache
        public static Hopcroft_Karp HK = new Hopcroft_Karp();
        private static StreamWriter writer;
        public static bool WriteMode = false;

        public static void WriteToLog(string s)
        {
            if (WriteMode)
                writer.WriteLine(s);
        }

        /// <summary>
        /// The main entry point for the application
        /// </summary>
        /// <param name="args">Command line variables</param>
        [STAThread]
        static void Main(string[] args)
        {


#if TRYCATCH
            try
            {
#endif

            //path = args[0];

            //Open a file dialog and let the user choose a graph to load
            string path;
            var fd = new OpenFileDialog();
            var ok = fd.ShowDialog();
            if (ok == DialogResult.OK)
                path = fd.FileName;
            else return;

            //Load the graph
            var graph = Graph.LoadFromDIMACS(path);
            Console.WriteLine("Graph loaded from " + path);
            graph = Graph.fromGrid(3, 3);
            //graph.Display("grid");
            Console.WriteLine("Graph has " + graph.vertices.Count + " vertices and " + graph.edges.Count + " edges.");
            var file = new FileStream("expresults.txt", FileMode.Append);
            writer = new StreamWriter(file);
            WriteMode = true;
            var matchers = new List<IMatchingAlgorithm>();
            matchers.Add(HK);
            matchers.Add(new fastMaximal());
            var constructors = new List<IConstructor>();
            var hs = new List<BottomUp.ConstructionHeuristic>();
            hs.Add(BottomUp.ConstructionHeuristic.bAllpairs);
            hs.Add(BottomUp.ConstructionHeuristic.bcompletelyRandom);
            hs.Add(BottomUp.ConstructionHeuristic.bRandomGreedy);
            hs.Add(BottomUp.ConstructionHeuristic.bSmallest);
            foreach (IMatchingAlgorithm alg in matchers)
                foreach (BottomUp.ConstructionHeuristic h in hs)
                    constructors.Add(new BottomUp(h, alg));
            foreach (IMatchingAlgorithm alg in matchers)
                constructors.Add(new SharminTreeBuilder(alg, true));
            var optimizers = new List<IOptimizer>();
            optimizers.Add(new SimulatedAnnealing(TreeBuilder.NeighborhoodOperator.twoswap, 14.4269504089d, (int)Math.Sqrt(graph.vertices.Count), 0.99f));
            optimizers.Add(new SimulatedAnnealing(TreeBuilder.NeighborhoodOperator.uncleSwap, 14.4269504089d, (int)Math.Sqrt(graph.vertices.Count), 0.99f));
            optimizers.Add(new IteratedLocalSearch(TreeBuilder.NeighborhoodOperator.twoswap));
            optimizers.Add(new IteratedLocalSearch(TreeBuilder.NeighborhoodOperator.uncleSwap));
            foreach (IMatchingAlgorithm alg in matchers)
            {
                optimizers.Add(new SharminTreeBuilder(alg, true));
                optimizers.Add(new SharminTreeBuilder(alg, false));
            }
            //var BU = new BottomUp(BottomUp.ConstructionHeuristic.bcompletelyRandom, HK);
            //var STB = new SharminTreeBuilder(HK, true);
            int iterator = 1;
            foreach (IConstructor cs in constructors)
                foreach (IOptimizer opt in optimizers)
                {
                    if (opt is SharminTreeBuilder)
                        if ((opt as SharminTreeBuilder).keepbalanced)
                            if (!(cs is SharminTreeBuilder))
                                continue;
                    iterator++;
                    Console.WriteLine("Running experiment " + iterator + " of " + (optimizers.Count - 1) * constructors.Count + 2);
                    writer.WriteLine("Experiment " + iterator + ": Graph: grid 3x3 , Construct: " + cs.Name + " , Optimize: " + opt.Name + " , Runtime: 1000000ms");
                    makeTree(graph, cs, opt, false, 1000000);
                    writer.Flush();
                    if (opt is SharminTreeBuilder)
                        (opt as SharminTreeBuilder).ClearCache();
                    HK.ClearCache();
                }
            file.Close();
            //writer.WriteLine("Experiment 1: Graph: Grid.10.10, Construct: BUCompletelyRandom, Optimize: SAtwoswap, Preprocessed, Runtime: 1000000ms");
            //makeTree(graph, BU, SA, true, 1000000);
            //writer.Flush();
            //file.Close();
          
#if TRYCATCH
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                return;
            }
#endif
            //Pause before exiting
            Console.ReadLine();
        }

        /// <summary>
        /// Makes a tree decomposition using the given heuristical operators
        /// </summary>
        /// <param name="g">The graph we want to decompose</param>
        /// <param name="cs">The construction heuristic</param>
        /// <param name="opt">The optimisation heuristic</param>
        /// <param name="preprocess">Whether or not the graph should be preprocessed</param>
        /// <param name="msToRun">How long our total building should take</param>
        /// <returns>A tree decomposition of g</returns>
        static Tree makeTree(Graph g, IConstructor cs, IOptimizer opt, bool preprocess, double msToRun)
        {
            Preprocessor pp = null;
            var starttime = DateTime.Now;
            if (preprocess)
            {
                pp = new Preprocessor();
                g = pp.preprocessGraph(g);
            }
           
            var T = cs.Construct(g);
            var constructtime = (DateTime.Now - starttime).TotalMilliseconds;
            WriteToLog("Initial tree of MM-width " + Hopcroft_Karp.GetMMWidth(g, T) + " constructed in " + constructtime + " milliseconds");
            Console.WriteLine("ITree " + Hopcroft_Karp.GetMMWidth(g, T) + " in " + constructtime + "ms");
            T.Display("originalTree");
            var timeleft = msToRun - constructtime;
            if (timeleft > 0 && opt != null)
            {
                T = opt.Optimize(g, T, timeleft);
            }
            if (preprocess)
                pp.completeTree(T);
            Console.WriteLine("Final tree of MM-width " + Hopcroft_Karp.GetMMWidth(g, T) + " constructed in " + (DateTime.Now - starttime).TotalMilliseconds + " milliseconds");
            WriteToLog("BestTree " + Hopcroft_Karp.GetMMWidth(g, T) + " in " + (DateTime.Now - starttime).TotalMilliseconds + " ms");
            return T;
        }

        
    }
}
