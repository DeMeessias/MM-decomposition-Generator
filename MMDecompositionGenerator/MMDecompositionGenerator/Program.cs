//Program.cs
//Gives the entry point for the application

#define TRYCATCH //if defined, exceptions that are thrown are displayed, after which the program exits
using System;
using System.Windows.Forms;
using System.IO;
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
        public static int numverts = 0;
        public static Graph graph;

        /// <summary>
        /// Writes a line to the output file, if write mode is on.
        /// </summary>
        /// <param name="s">The line to write</param>
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

            //Open a file dialog and let the user choose a graph to load
            string path;
            string outputfilename = "output.txt";
            var fd = new OpenFileDialog();
            var ok = fd.ShowDialog();
            if (ok == DialogResult.OK)
                path = fd.FileName;
            else return;

            //Load the graph
            graph = Graph.LoadFromDIMACS(path);
            Console.WriteLine("Graph loaded from " + path);
            numverts = graph.vertices.Count;
            Console.WriteLine("Graph has " + graph.vertices.Count + " vertices and " + graph.edges.Count + " edges.");
            var file = new FileStream(outputfilename, FileMode.Append);
            writer = new StreamWriter(file);
            WriteMode = true;

            var constructor = new BottomUp(BottomUp.ConstructionHeuristic.bcompletelyRandom, HK);
            var improver = new SimulatedAnnealing(TreeBuilder.NeighborhoodOperator.twoswap, 15, (int)Math.Sqrt(numverts), 0.99f);
            makeTree(graph, constructor, improver, true, 1000000);
            writer.Flush();
            file.Close();


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
