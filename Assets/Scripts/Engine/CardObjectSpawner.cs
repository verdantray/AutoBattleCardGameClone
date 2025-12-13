using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectABC.Core;
using UnityEngine;

namespace ProjectABC.Engine
{
    public class CardSpawnArgs
    {
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly CardReference CardReference;

        public CardSpawnArgs(Transform target, CardReference card = null)
        {
            Position =  target.position;
            Rotation = target.rotation;
            CardReference = card;
        }

        public CardSpawnArgs(Vector3 position, Quaternion rotation, CardReference card = null)
        {
            Position = position;
            Rotation = rotation;
            CardReference = card;
        }
    }

    public abstract class CardObject : PoolableObject, ISpawnable<CardSpawnArgs>
    {
        [SerializeField] private int poolSize;
        
        public override int PoolSize => poolSize;
        
        public abstract void OnSpawned(CardSpawnArgs args);

        public abstract void OnDespawned();
        
        protected abstract void ApplyArgs(CardSpawnArgs args);
    }
    
    public sealed class CardObjectSpawner : MonoBehaviour, IObjectSpawner<CardSpawnArgs>
    {
        [SerializeField] private string[] cardAddressableNames;
        
        private readonly Dictionary<string, ObjectPooler> _poolers = new Dictionary<string, ObjectPooler>();

        private void OnEnable()
        {
            InitializeSpawner();
        }

        private void InitializeSpawner()
        {
            foreach (var cardAddressableName in cardAddressableNames)
            {
                if (string.IsNullOrEmpty(cardAddressableName))
                {
                    continue;
                }
                
                if (_poolers.TryGetValue(cardAddressableName, out var pooler))
                {
                    if (!pooler.IsValid())
                    {
                        pooler.InitializePooler(cardAddressableName);
                    }
                    
                    continue;
                }
                
                pooler = MakePooler(cardAddressableName);
                _poolers.Add(cardAddressableName, pooler);
            }
        }

        private ObjectPooler MakePooler(object key)
        {
            GameObject poolerObject = new GameObject();
            poolerObject.transform.SetParent(transform);
            
            ObjectPooler pooler = poolerObject.AddComponent<ObjectPooler>();
            pooler.InitializePooler(key);
            
            return pooler;
        }

        public Task GetInitializingTask()
        {
            return Task.WhenAll(_poolers.Values.Select(pooler => pooler.GetInitializingTask()));
        }
        
        public T Spawn<T>(string objectName, CardSpawnArgs args) where T : PoolableObject, ISpawnable<CardSpawnArgs>
        {
            if (!_poolers.TryGetValue(objectName, out var pooler))
            {
                Debug.LogWarning($"{objectName} can't spawn, not pooled");
                return null;
            }

            PoolableObject pooled = pooler.Get();

            T cardObject = pooled.GetComponent<T>();
            cardObject.gameObject.SetActive(true);
            cardObject.OnSpawned(args);

            return cardObject;
        }

        public void Despawn<T>(T toDespawn) where T : PoolableObject, ISpawnable<CardSpawnArgs>
        {
            string objectName = toDespawn.gameObject.name;

            if (!_poolers.TryGetValue(objectName, out var pooler))
            {
                Debug.LogWarning($"{objectName} can't despawn, not pooled");
                return;
            }
            
            pooler.Release(toDespawn);
        }
    }
}
