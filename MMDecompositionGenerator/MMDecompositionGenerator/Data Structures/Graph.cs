using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MMDecompositionGenerator.Data_Structures
{
    /// <summary>
    /// Represents a connected, simple graph
    /// </summary>
    class Graph
    {
        Dictionary<int,Vertex> vertices;
        Dictionary<Tuple<int,int>,Edge> edges;

        public Dictionary<int,Vertex> Vertices
        {
            get { return vertices; }
        }

        public Dictionary<Tuple<int,int>,Edge> Edges
        {
            get { return edges; }
        }

        public int V
        { get { return vertices.Count; } }
        public int E
        { get { return edges.Count; } }

        public Graph()
        {
            vertices = new Dictionary<int, Vertex>();
            edges = new Dictionary<Tuple<int, int>, Edge>();
        }

        private void DisconnectVertices(Vertex u, Vertex v)
        {
            u.RemoveConnection(v);
            v.RemoveConnection(u);
            edges.Remove(new Tuple<int,int>(u.Index,v.Index));
        }

        private void ConnectVertices(Vertex u, Vertex v)
        {
            u.AddConnection(v);
            v.AddConnection(u);
            var e = new Edge(u, v);
            edges.Add(new Tuple<int, int>(e.u.Index, e.v.Index), e);
        }

        public static Graph LoadFromDIMACS(string path)
        {
            var g = new Graph();
            var file = new FileStream(path, FileMode.Open);
            var reader = new StreamReader(file);
            string str;
            int v = -1;
            int e = -1;
            while ((str = reader.ReadLine()) != null)
            {
                var line = str.Split();
                if (line[0] == "p")
                {
                    v = int.Parse(line[2]);
                    e = int.Parse(line[3]);
                    for (int i = 1; i < v + 1; i++)
                    {
                        var vert = new Vertex(i);
                        g.Vertices.Add(i, vert);
                    }
                }
                if (line[0] == "e")
                {
                    int ui = int.Parse(line[1]);
                    int vi = int.Parse(line[2]);
                    g.ConnectVertices(g.Vertices[ui], g.Vertices[vi]);
                }
            }
            if (g.V != v || g.E != e)
            {
                throw new Exception("Error in DIMACS file");
            }
            return g;
        }
    }
}
