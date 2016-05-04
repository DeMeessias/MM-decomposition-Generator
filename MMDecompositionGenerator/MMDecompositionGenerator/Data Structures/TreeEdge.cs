using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMDecompositionGenerator.Data_Structures
{
    class TreeEdge : IEquatable<TreeEdge>
    {
        TreeVertex u, v;

        /// <summary>
        /// Makes a new TreeEdge object. The "first" vertex in the edge is always the parent.
        /// </summary>
        /// <param name="u">The parent vertex</param>
        /// <param name="v">The child vertex</param>
        public TreeEdge(TreeVertex u, TreeVertex v)
        {
                this.u = u;
                this.v = v;
        }

        public bool Equals(TreeEdge other)
        {
            if (this.u == other.u && this.v == other.v)
                return true;
            else return false;
        }
    }
}
