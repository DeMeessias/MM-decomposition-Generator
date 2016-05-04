using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMDecompositionGenerator.Data_Structures
{
    /// <summary>
    /// Represents a tree
    /// </summary>
    class Tree
    {
        List<TreeVertex> vertices;
        List<TreeEdge> edges;

        public Tree()
        {
            vertices = new List<TreeVertex>();
            edges = new List<TreeEdge>();
        }

        private void ConnectChild(TreeVertex parent, TreeVertex child)
        {
            parent.AddChild(child);
            child.AddParent(parent);
            edges.Add(new TreeEdge(parent, child));
        }

        private void DisconnectChild(TreeVertex parent, TreeVertex child)
        {
            parent.RemoveChild(child);
            child.RemoveParent();
            edges.Remove(new TreeEdge(parent, child));
        }
    }
}
