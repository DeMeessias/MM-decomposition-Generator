//Graph.cs
//Defines our graph class, and the FileDotEngine helper class needed for our display methods
//Uses Quickgraph (http://quickgraph.codeplex.com/), GraphViz(http://www.graphviz.org/) and a wrapper for graphviz called GraphViz.NET(https://github.com/JamieDixon/GraphViz-C-Sharp-Wrapper)
using GraphVizWrapper;
using QuickGraph;
using QuickGraph.Graphviz;
using QuickGraph.Graphviz.Dot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace MMDecompositionGenerator.Data_Structures
{
    /// <summary>
    /// Represents a connected, simple graph
    /// </summary>
    class Graph
    {
        //Lists of edges and vertices
        public Dictionary<int,Vertex> vertices;
        public List<Edge> edges;

        /// <summary>
        /// Constructs a new, empty graph
        /// </summary>
        public Graph()
        {
            vertices = new Dictionary<int,Vertex>();
            edges = new List<Edge>();
        }

        /// <summary>
        /// Constructs a graph that is a copy of another graph (with unique vertex and edge elements)
        /// </summary>
        /// <param name="g">The graph we want to copy</param>
        public Graph(Graph g)
        {
            vertices = new Dictionary<int, Vertex>();
            edges = new List<Edge>();
            foreach (Vertex v in g.vertices.Values)
                vertices.Add(v.Index, new Vertex(v.Index));
            foreach (Edge e in g.edges)
            {
                ConnectVertices(vertices[e.u.Index], vertices[e.v.Index]);
            }
        }

        /// <summary>
        /// Removes the connection between two vertices
        /// </summary>
        /// <param name="u">One of the edges to be disconnected</param>
        /// <param name="v">The other edge to be disconnected</param>
        public void DisconnectVertices(Vertex u, Vertex v)
        {

            var e = new Edge(u, v);
            u.neighbors.Remove(v);
            u.incedentEdges.Remove(e);
            v.neighbors.Remove(u);
            v.incedentEdges.Remove(e);
            if (!edges.Remove(e))
                throw new Exception("Error trying to remove edge");
        }

        /// <summary>
        /// Connects two vertices with an edge
        /// </summary>
        /// <param name="u">One of the two vertices to be connected</param>
        /// <param name="v">The other vertex to be connected</param>
        public virtual void ConnectVertices(Vertex u, Vertex v)
        {
            var e = new Edge(u, v);
            u.neighbors.Add(v);
            v.neighbors.Add(u);
            u.incedentEdges.Add(e);
            v.incedentEdges.Add(e);
            edges.Add(e);
        }

        /// <summary>
        /// Loads a graph from a DIMACS file
        /// </summary>
        /// <param name="path">The path to the file</param>
        /// <returns>The graph encoded in the file</returns>
        public static Graph LoadFromDIMACS(string path)
        {
            //Open the file
            var g = new Graph();
            var file = new FileStream(path, FileMode.Open);
            var reader = new StreamReader(file);

            string str;
            int v = -1;
            int e = -1;

            //Read the file line by line
            while ((str = reader.ReadLine()) != null)
            {
                var line = str.Split();

                //Handle the problem line
                if (line[0] == "p")
                {
                    v = int.Parse(line[2]);
                    e = int.Parse(line[3]);
                }

                //Handle a node specification (not present in every file)
                if (line[0] == "n")
                {
                    var vert = new Vertex(int.Parse(line[1]));
                    g.vertices.Add(vert.Index,vert);
                }

                //Hande an edge specification
                if (line[0] == "e")
                {
                    //If the vertices have not been specified separately in the file, generate vertices indexed 1...n
                    if (g.vertices.Count == 0)
                        for (int i = 1; i < v + 1; i++)
                        {
                            var vert = new Vertex(i);
                            g.vertices.Add(vert.Index,vert);
                        }

                    int ui = int.Parse(line[1]);
                    int vi = int.Parse(line[2]);
                    g.ConnectVertices(g.vertices[ui], g.vertices[vi]);
                }
            }
            if (g.vertices.Count != v || g.edges.Count != e)
            {
                throw new Exception("Error in DIMACS file");
            }
            return g;
        }

        /// <summary>
        /// Save an image of the graph using Quickgraph and GraphViz
        /// </summary>
        /// <param name="fileName">The name of the file to be generated</param>
        public void Display(string fileName)
        {
            //Transform our graph into a quickgraph graph
            var quickGraph = new AdjacencyGraph<Vertex, Edge>();
            quickGraph.AddVerticesAndEdgeRange(edges);
            var graphviz = new GraphvizAlgorithm<Vertex, Edge>(quickGraph);
            graphviz.FormatVertex += new FormatVertexEventHandler<Vertex>(FormatVertex);
            //generate a dotfile               
            string dots = graphviz.Generate(new FileDotEngine(), fileName);

            //Turn the dotfile into a png image using graphviz
            var getStartProcessQuery = new GraphVizWrapper.Queries.GetStartProcessQuery();
            var getProcessStartInfoQuery = new GraphVizWrapper.Queries.GetProcessStartInfoQuery();
            var registerLayoutPluginCommand = new GraphVizWrapper.Commands.RegisterLayoutPluginCommand(getProcessStartInfoQuery, getStartProcessQuery);
            var wrapper = new GraphGeneration(getStartProcessQuery,
                                              getProcessStartInfoQuery,
                                              registerLayoutPluginCommand);
            var dotsfile = new StreamReader(dots);
            string alles = dotsfile.ReadToEnd();           
            var output = wrapper.GenerateGraph(alles, Enums.GraphReturnType.Png);
            var ms = new MemoryStream(output);
            var img = Image.FromStream(ms);
            img.Save(fileName + ".png", System.Drawing.Imaging.ImageFormat.Png);
            Console.WriteLine("Graph visualized");
            
        }

        /// <summary>
        /// Helper method for formating vertices, to be used by Display
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The set of arguments to be formatted</param>
        private static void FormatVertex(object sender, FormatVertexEventArgs<Vertex> e)
        {
            Vertex v = e.Vertex;
            e.VertexFormatter.Label = v.Index.ToString();
        }

        /// <summary>
        /// Generates a graph that is a grid with the given dimensions
        /// </summary>
        /// <param name="n">The length of the graph</param>
        /// <param name="m">The width of the graph</param>
        /// <returns>A graph that is a grid of size n x m</returns>
        public static Graph fromGrid(int n, int m)
        {
            if (n >= 1000 || m >= 1000)
                throw new Exception("Grid is too large");
            var g = new Graph();
            //var grid = new List<List<Vertex>>();
            for (int i = 0; i < n; i++)
                for (int j = 0; j < m; j++)
                {
                    var v = new Vertex(1000 * i + j);
                    g.vertices.Add(v.Index, v);
                    if (i != 0)
                        g.ConnectVertices(g.vertices[1000 * (i - 1) + j], v);
                    if (j != 0)
                        g.ConnectVertices(g.vertices[1000 * i + j - 1], v);
                        
                }
            if (g.vertices.Values.Count != n * m || g.edges.Count != n * (m-1) + m * (n-1))
                throw new Exception("Error constructing grid");

            return g;
        }
    }


    /// <summary>
    /// Class used for generating a dotfile. Needed by the Display method
    /// </summary>
        class FileDotEngine : IDotEngine
    {
      
        /// <summary>
        /// Runs the file dot engine
        /// </summary>
        /// <param name="imageType">The image type</param>
        /// <param name="dot">The name of the dot file</param>
        /// <param name="outputFileName">The name of the output file</param>
        /// <returns></returns>
           public string Run(GraphvizImageType imageType, string dot, string outputFileName)
        {
            using (StreamWriter writer = new StreamWriter(outputFileName))
            {
                writer.Write(dot);
            }

            return Path.GetFileName(outputFileName);
        }
    }
}