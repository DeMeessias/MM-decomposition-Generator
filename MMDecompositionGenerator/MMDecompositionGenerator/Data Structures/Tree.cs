//Tree.cs
//Defines our tree class
//Uses Quickgraph (http://quickgraph.codeplex.com/), GraphViz(http://www.graphviz.org/) and a wrapper for graphviz called GraphViz.NET(https://github.com/JamieDixon/GraphViz-C-Sharp-Wrapper)
using System;
using System.Collections.Generic;
using QuickGraph;
using QuickGraph.Graphviz;
using System.Drawing;
using GraphVizWrapper;
using System.IO;
using System.Linq;

namespace MMDecompositionGenerator.Data_Structures
{
    /// <summary>
    /// Represents a tree
    /// </summary>
    class Tree
    {
        private TreeVertex root;
        public TreeVertex Root
        {
            get
            {
                if (root == null || root.parent != null)
                    root = getRoot();
                return root;
            }
        }
        //Lists of edges and vertices
        List<TreeVertex> vertices;
        List<TreeEdge> edges;

        //Property for accessing the vertex list
        public List<TreeVertex> Vertices
        {
            get { return vertices; }
        }

        //Property for accessing the edge list
        public List<TreeEdge> Edges { get { return edges; } }

        /// <summary>
        /// Constructs a new, empty tree object. Ideally only called by methods in the treebuilder class
        /// </summary>
        public Tree()
        {
            vertices = new List<TreeVertex>();
            edges = new List<TreeEdge>();
        }

        /// <summary>
        /// Connects two vertices with an edge
        /// </summary>
        /// <param name="parent">The vertex that will become the parent</param>
        /// <param name="child">The vertex that will become the child</param>
        public void ConnectChild(TreeVertex parent, TreeVertex child)
        {
            parent.children.Add(child);
            parent.neighbors.Add(child);
            child.parent = parent;
            child.neighbors.Add(parent);
            var e = new TreeEdge(parent, child);
            parent.incedentEdges.Add(e);
            child.incedentEdges.Add(e);
            edges.Add(e);
        }

        /// <summary>
        /// Removes the connection between two vertices
        /// </summary>
        /// <param name="parent">The parent vertex</param>
        /// <param name="child">The child vertex</param>
        public void DisconnectChild(TreeVertex parent, TreeVertex child)
        {
            parent.children.Remove(child);
            parent.neighbors.Remove(child);
            if (child.parent == parent)
                child.parent = null;
            child.neighbors.Remove(parent);
            var e = new TreeEdge(parent, child);
            parent.incedentEdges.Remove(e);
            child.incedentEdges.Remove(e);
            edges.Remove(e);
        }

        /// <summary>
        /// Saves an image of the tree, using Quickgraph and Graphviz
        /// </summary>
        /// <param name="fileName">The name of the file the image should be saved to</param>
        public void Display(string fileName)
        {
            //Convert our tree to a quickgraph graph
            var quickGraph = new AdjacencyGraph<TreeVertex, TreeEdge>();
            quickGraph.AddVerticesAndEdgeRange(Edges);
            var graphviz = new GraphvizAlgorithm<TreeVertex, TreeEdge>(quickGraph);
            graphviz.FormatVertex += new FormatVertexEventHandler<TreeVertex>(FormatVertex);
            //generate a dotfile
            string dots = graphviz.Generate(new FileDotEngine(), fileName);
      
            //turn the dotfile into a png file
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
            Console.WriteLine("Tree visualized");
        }

        /// <summary>
        /// Helper method for formatting a vertex. Used by the Display method
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The arguments to be formatted</param>
        private void FormatVertex(object sender, FormatVertexEventArgs<TreeVertex> e)
        {
            //Include all bijected vertices on the label
            TreeVertex v = e.Vertex;
            e.VertexFormatter.Label = "";
            foreach (Vertex bv in v.bijectedVertices)
                e.VertexFormatter.Label = e.VertexFormatter.Label + bv.Index + " ";
        }

        /// <summary>
        /// Finds and returns the root of the tree
        /// </summary>
        /// <returns>The TreeVertex that is the root of the tree</returns>
        private TreeVertex getRoot()
        {
            var roots = new List<TreeVertex>();
            foreach (TreeVertex tv in Vertices)
                if (tv.parent == null)
                    roots.Add(tv);
            if (roots.Count != 1)
                throw new Exception("Error finding root");
            return roots[0];
        }

        /// <summary>
        /// Replaces a part of the tree by the given subtree
        /// </summary>
        /// <param name="t">The subtree we want to connect</param>
        public void AppendSubtree(Tree t)
        {
            var subRoot = t.Root;
            var connectLeaf = findVertex(subRoot.bijectedVertices);
            if (connectLeaf == null)
                throw new Exception("Error appending subtree");
            var connectParent = connectLeaf.parent;
            foreach (TreeVertex desc in connectLeaf.Descendants)
            {
                Vertices.Remove(desc);
                DisconnectChild(desc.parent, desc);                
            }
            foreach (TreeVertex tv in t.Vertices)
                Vertices.Add(tv);
            foreach (TreeEdge te in t.Edges)
                Edges.Add(te);
            ConnectChild(connectParent, subRoot);
        }

        /// <summary>
        /// Finds a vertex in the tree that represents the given partition
        /// </summary>
        /// <param name="partition">The partition we want to look for</param>
        /// <returns>The treevertex representing the parition if it exists, null otherwise</returns>
        public TreeVertex findVertex(List<Vertex> partition)
        {
            partition.Sort();
            var lv = Root;
            var pc = new Algorithms.PartComparer();
            while (true)
            {
                var prelv = lv;
                lv.bijectedVertices.Sort();
                if (pc.Equals(lv.bijectedVertices, partition))
                    return lv;
                foreach (TreeVertex c in lv.children)
                 if (partition.Except(c.bijectedVertices).ToList().Count == 0)
                  lv = c;
                if (lv == prelv)
                    return null;                         
            }
        }
    }

}
