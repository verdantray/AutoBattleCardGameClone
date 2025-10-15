using System.Threading.Tasks;
using UnityEngine;

namespace ProjectABC.Engine
{
    public interface IPoolable
    {
        public int PoolSize { get; }
    }

    public abstract class PoolableObject : MonoBehaviour, IPoolable
    {
        public abstract int PoolSize { get; }
    }

    public interface IPooler
    {
        public void InitializePooler(object key);
        public Task GetInitializingTask();
        public void DestroyPool();
        public void Prewarm(int count);
        public PoolableObject Get();
        public void Release(PoolableObject toRelease);
    }
}
