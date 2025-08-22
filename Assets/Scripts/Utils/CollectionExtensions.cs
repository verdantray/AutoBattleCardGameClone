using System.Collections;

namespace ProjectABC.Utils
{
    public static class CollectionExtensions
    {
        public static void Shuffle(this IList list, int? seed = null)
        {
            System.Random random = seed != null
                ? new System.Random(seed.Value)
                : new System.Random();

            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}