using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ProjectABC.Engine
{
    public sealed class ObjectPooler : MonoBehaviour, IPooler
    {
        private AsyncOperationHandle<GameObject> _handle;
        private PoolableObject _objectToPool;
        private ObjectPool<PoolableObject> _pool;

        private void Awake()
        {
            NamePooler();
        }

        private void OnDestroy()
        {
            DestroyPool();
        }

        public void NamePooler()
        {
            gameObject.name = _objectToPool != null
                ? $"Pool_{_objectToPool.name}"
                : $"Pool_empty";
        }

        public bool IsValid() => _handle.IsValid();
        
        public Task GetInitializingTask()
        {
            return _handle.Task;
        }

        public void InitializePooler(object key)
        {
            DestroyPool();

            _handle = Addressables.LoadAssetAsync<GameObject>(key);
            _handle.Completed += OnHandleCompleted;
        }

        public PoolableObject Get()
        {
            return _pool.Get();
        }

        public void Release(PoolableObject toRelease)
        {
            _pool.Release(toRelease);
        }

        private void OnHandleCompleted(AsyncOperationHandle<GameObject> handle)
        {
            CreatePool(handle.Result);
            NamePooler();
        }

        public void DestroyPool()
        {
            if (_pool != null)
            {
                _pool.Dispose();
                _pool = null;
            }

            if (IsValid())
            {
                _objectToPool = null;
                _handle.Release();
            }
            
            NamePooler();
        }

        private void CreatePool(GameObject objectToPool)
        {
            _objectToPool = objectToPool.GetComponent<PoolableObject>();
            _objectToPool.gameObject.SetActive(false);
            
            _pool = new ObjectPool<PoolableObject>(CreateInstance, OnGet, OnRelease);

            int poolSize = _objectToPool.PoolSize;
            Prewarm(poolSize);
        }

        public void Prewarm(int count)
        {
            List<PoolableObject> created = new List<PoolableObject>();

            for (int i = 0; i < count; i++)
            {
                var gotObject = _pool.Get();
                created.Add(gotObject);
            }

            foreach (var obj in created)
            {
                _pool.Release(obj);
            }
        }

        private PoolableObject CreateInstance()
        {
            PoolableObject instance = Instantiate(_objectToPool, transform);

            instance.name = _objectToPool.name;
            instance.gameObject.SetActive(false);

            return instance;
        }

        private void OnGet(PoolableObject gotObject)
        {
            // do nothing
        }

        private void OnRelease(PoolableObject releasedObject)
        {
            releasedObject.gameObject.SetActive(false);
        }
    }
}
