//Program.cs
//Gives the entry point for the application

//#define TRYCATCH //if defined, exceptions that are thrown are displayed, after which the program exits
using System;
using System.Windows.Forms;
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
            var graph = Data_Structures.Graph.LoadFromDIMACS(path);
            Console.WriteLine("Graph loaded from " + path);
            graph = Graph.fromGrid(3, 3);
            graph.Display("grid");
            Console.WriteLine("Graph has " + graph.vertices.Count + " vertices and " + graph.edges.Count + " edges.");

            

            var STB = new SharminTreeBuilder(HK, true);
            var SA = new SimulatedAnnealing(TreeBuilder.NeighborhoodOperator.uncleSwap, 1000, 10, 0.95f);
            makeTree(graph, STB, SA, true, 50000);
            //Generate a tree from the graph
            //var stb = new Algorithms.SharminTreeBuilder();
            //var T = stb.ConstructNewTree(graph);
            //var T = stb.ConstructNewTree(graph, HK);
            //var T = Data_Structures.TreeBuilder.fromGraph(graph, Data_Structures.TreeBuilder.Heuristic.bcompletelyRandom,Data_Structures.TreeBuilder.NeighborhoodOperator.uncleSwap, true,Program.HK);
            //Console.WriteLine("MM-width of generated tree: " + Algorithms.Hopcroft_Karp.GetMMWidth(graph, T));
            //T = stb.Optimize(graph, T, 100000, false, HK);
            //T.Display("tree");
            //T = Data_Structures.TreeBuilder.TimedIteratedLocalSearch(graph, T, Data_Structures.TreeBuilder.NeighborhoodOperator.twoswap, 100000);     
            //T = Data_Structures.TreeBuilder.SimulatedAnnealing(graph, T, Data_Structures.TreeBuilder.NeighborhoodOperator.twoswap, 100, 10, 0.95f, 100000);       
            //Console.WriteLine("MM-width of improved tree: " + Algorithms.Hopcroft_Karp.GetMMWidth(graph, T));
          
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
            Console.WriteLine("Initial tree of MM-width " + Hopcroft_Karp.GetMMWidth(g, T) + " constructed in " + constructtime + " milliseconds");
            var timeleft = msToRun - constructtime;
            if (timeleft > 0 && opt != null)
            {
                T = opt.Optimize(g, T, timeleft);
            }
            if (preprocess)
                pp.completeTree(T);
            Console.WriteLine("Final tree of MM-width " + Hopcroft_Karp.GetMMWidth(g, T) + " constructed in " + (DateTime.Now - starttime).TotalMilliseconds + " milliseconds");
            return T;
        }
    }
}
