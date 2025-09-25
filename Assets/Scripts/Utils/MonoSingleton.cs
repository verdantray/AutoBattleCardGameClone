using UnityEngine;


namespace ProjectABC.Utils
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; protected set; }
        public static bool HasInstance => (bool)Instance;
        protected bool IsInstance => HasInstance && Instance == this as T;

        protected virtual void Awake()
        {
            InitializeSingleton();
        }

        protected void InitializeSingleton()
        {
            if (HasInstance && !IsInstance)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
    }
}