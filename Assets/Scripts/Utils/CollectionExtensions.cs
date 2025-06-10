using System.Collections.Generic;
using UnityEngine;

namespace ProjectABC.Utils
{
    public static class CollectionExtensions
    {
        public static void Shuffle<T>(IList<T> list)
        {
            var count = list.Count;
            var last = count - 1;
            for (var i = 0; i < last; ++i)
            {
                var r = Random.Range(i, count);
                (list[i], list[r]) = (list[r], list[i]);
            }
        }
    }
}