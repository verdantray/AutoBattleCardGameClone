

namespace ProjectABC.Utils
{
    public abstract class Singleton<T> where T : class, new()
    {
        public static T Instance { get; private set; }
        public static bool HasInstance => Instance != null;

        protected static T CreateInstanceInternal()
        {
            if (!HasInstance)
            {
                Instance = new T();
            }

            return Instance;
        }
    }
}
