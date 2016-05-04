using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMDecompositionGenerator.Data_Structures
{
    class TreeVertex
    {
        List<TreeEdge> incedentEdges;
        List<TreeVertex> neighbors;
        TreeVertex parent;
        List<TreeVertex> children;

        public List<TreeVertex> Descendants
        {
            get { var d = new List<TreeVertex>();
                var q = new Queue<TreeVertex>();
                q.Enqueue(this);
                while (q.Count != 0)
                {
                    var v = q.Dequeue();
                    foreach (TreeVertex tv in v.children)
                        q.Enqueue(tv);
                    d.Add(v);
                }
                return d;
            }
        }

        public TreeVertex()
        {
            incedentEdges = new List<TreeEdge>();
            neighbors = new List<TreeVertex>();
            parent = null;
            children = new List<TreeVertex>();
        }

        public void AddChild(TreeVertex v)
        {
        children.Add(v);
        neighbors.Add(v);
        incedentEdges.Add(new TreeEdge(this, v));
        }

        public void RemoveChild(TreeVertex v)
        {
            children.Remove(v);
            neighbors.Remove(v);
            incedentEdges.Remove(new TreeEdge(this, v));
        }

        public void AddParent(TreeVertex v)
        {
            parent = v;
            neighbors.Add(v);
            incedentEdges.Add(new TreeEdge(v, this));
        }

        public void RemoveParent()
        {
            neighbors.Remove(parent);
            incedentEdges.Remove(new TreeEdge(parent, this));
            parent = null;
        }

    }
}
