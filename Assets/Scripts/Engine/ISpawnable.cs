
namespace ProjectABC.Engine
{
    public interface ISpawnable<in T> where T : class
    {
        public void OnSpawned(T args);
        public void OnDespawned();
    }

    public interface IObjectSpawner<T> where T : class
    {
        public TObject Spawn<TObject>(string objectName, T args) where TObject : PoolableObject, ISpawnable<T>;
        public void Despawn<TObject>(TObject toDespawn) where  TObject : PoolableObject, ISpawnable<T>;
    }
}
