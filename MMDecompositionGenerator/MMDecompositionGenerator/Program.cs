using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMDecompositionGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Data_Structures.Graph graph;
            string path = "C:/Users/DeMeessias/Documents/1. Studie/Experimentation project/Graphs/1gcq_graph.dimacs";
            
            try {
                //path = args[0];
                graph = Data_Structures.Graph.LoadFromDIMACS(path);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                return;
            }
            Console.WriteLine("Graph loaded from " + path);
            Console.WriteLine("Graph has " + graph.V + " vertices and " + graph.E + " edges.");
            Console.ReadLine();
        }
    }
}
