//PartComparer.cs
//Helper class for Hopcroft-Karp's cache. Compares two partitions and sees if they are the same.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMDecompositionGenerator.Data_Structures;

namespace MMDecompositionGenerator.Algorithms
{
    /// <summary>
    /// Equality comparer for partitions of vertices
    /// </summary>
    class PartComparer : IEqualityComparer<List<Vertex>>
    {
        /// <summary>
        /// Compares two partitions of vertices and checks if they are equal
        /// </summary>
        /// <param name="x">paritition A</param>
        /// <param name="y">partition B</param>
        /// <returns></returns>
        public bool Equals(List<Vertex> x, List<Vertex> y)
        {
            return x.SequenceEqual(y);
        }

        /// <summary>
        /// Gets the hashcode for a partition. Equal to the index of the tree-vertex representing the partition
        /// </summary>
        /// <param name="obj">The partition</param>
        /// <returns></returns>
        public int GetHashCode(List<Vertex> obj)
        {

            if (obj.Count == 1)
                return obj[0].Index;
            else
            {
                int i = 0;
                foreach (Vertex v in obj)
                    i += (int)Math.Pow(v.Index + 100, 2);
                return i;
            }
        }
    }
}
