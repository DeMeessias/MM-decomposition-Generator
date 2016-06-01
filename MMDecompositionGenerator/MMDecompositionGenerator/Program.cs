//Program.cs
//Gives the entry point for the application

//#define TRYCATCH //if defined, exceptions that are thrown are displayed, after which the program exits
using System;
using System.Windows.Forms;

namespace MMDecompositionGenerator
{
    /// <summary>
    /// Class for holding the main method and defining global variables
    /// </summary>
    class Program
    {
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
            Console.WriteLine("Graph has " + graph.vertices.Count + " vertices and " + graph.edges.Count + " edges.");
            //Data_Structures.Graph.DisplayGraph(graph);

            //Generate a tree from the graph
            var T = Data_Structures.TreeBuilder.fromGraph(graph, Data_Structures.TreeBuilder.Heuristic.bcompletelyRandom,Data_Structures.TreeBuilder.NeighborhoodOperator.uncleSwap, true);
            Console.WriteLine("MM-width of generated tree: " + Algorithms.Hopcroft_Karp.GetMMWidth(graph, T));
            //T.Display("tree");
            //T = Data_Structures.TreeBuilder.SimulatedAnnealing(graph, T, Data_Structures.TreeBuilder.NeighborhoodOperator.uncleSwap, 100, (int)Math.Ceiling((double)graph.vertices.Count / 30), 0.95f, 100000);
            T = Data_Structures.TreeBuilder.IteratedLocalSearch(graph, T, Data_Structures.TreeBuilder.NeighborhoodOperator.uncleSwap, 100000);            
            Console.WriteLine("MM-width of improved tree: " + Algorithms.Hopcroft_Karp.GetMMWidth(graph, T));
            T = Data_Structures.TreeBuilder.SimulatedAnnealing(graph, T, Data_Structures.TreeBuilder.NeighborhoodOperator.twoswap, 100, 10, 0.95f, 100000);
            Console.WriteLine("MM-width of improved tree: " + Algorithms.Hopcroft_Karp.GetMMWidth(graph, T));
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
    }
}
