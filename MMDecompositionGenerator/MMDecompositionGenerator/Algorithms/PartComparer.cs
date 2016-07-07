//PartComparer.cs
//Helper class for Hopcroft-Karp's and SharminTreeBuilder's caches. Compares two partitions and sees if they are the same.
using System;
using System.Collections;
using System.Collections.Generic;
using MMDecompositionGenerator.Data_Structures;

namespace MMDecompositionGenerator.Algorithms
{
    /// <summary>
    /// Equality comparer for partitions of vertices
    /// </summary>
    class PartComparer : IEqualityComparer<BitArray>
    {
        /// <summary>
        /// Compares two partitions of vertices and checks if they are equal
        /// </summary>
        /// <param name="x">paritition A</param>
        /// <param name="y">partition B</param>
        /// <returns></returns>
        public bool Equals(BitArray x, BitArray y)
        {
            if (x.Count != y.Count)
                return false;
            for (int i = 0; i < x.Count; i++)
                if (x[i] != y[i])
                    return false;
            return true;
        }

        /// <summary>
        /// Gets the hashcode for a partition. Equal to the index of the tree-vertex representing the partition
        /// </summary>
        /// <param name="obj">The partition</param>
        /// <returns></returns>
        public int GetHashCode(BitArray obj)
        {
            int worlen = 31;
            int numwords = obj.Count / worlen;
            int leftover = obj.Count % worlen;
            long hashcode = 0;
            for (int i = 0; i < numwords; i++)
                for (int j = 0; j < worlen; j++)
                    if (obj[i * worlen + j])
                        hashcode = (hashcode + (int)Math.Pow(j, 2)) % int.MaxValue;
            if (leftover > 0)
                for (int i = 0; i < leftover; i++)
                    if (obj[numwords * worlen + i])
                        hashcode = (hashcode + (int)Math.Pow(i, 2)) % int.MaxValue;
            return (int)hashcode;
        }
    }
}
