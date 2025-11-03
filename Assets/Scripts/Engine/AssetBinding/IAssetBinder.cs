using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ProjectABC.Engine
{
    public interface IAssetBinder
    {
        public Task GetAssetLoadingTask();
        public bool IsAllBindingHandlesLoaded();

        public static bool CheckHandleLoaded(AsyncOperationHandle handle)
        {
            return handle.IsValid() && handle.Status == AsyncOperationStatus.Succeeded;
        }
    }

    public interface IAssetBindEntry
    {
        
    }
    
    public interface IAssetBinder<in T> : IAssetBinder where T : IAssetBindEntry, IEquatable<T>
    {
        public void AddAssetBindingEntry(T entry);
        public bool RemoveAssetBindingEntry(T entry);
        public bool TryGetBindingHandle(T entry, out AsyncOperationHandle handle);
        public void BindAsset(T entry);
        public void ReleaseAsset(T entry);
    }

    public abstract class AssetHandleSchedule<T> where T : IAssetBindEntry, IEquatable<T>
    {
        public virtual bool Precondition(IAssetBinder<T> assetBinder) => true;
        public abstract void Run(IAssetBinder<T> assetBinder);
    }

    public sealed class AssetBindSchedule<T> : AssetHandleSchedule<T> where T : IAssetBindEntry, IEquatable<T>
    {
        private readonly List<T> _entries = new List<T>();
        
        public AssetBindSchedule(T entry)
        {
            _entries.Add(entry);
        }

        public AssetBindSchedule(IList<T> entries)
        {
            _entries.AddRange(entries);
        }
        
        public override void Run(IAssetBinder<T> assetBinder)
        {
            foreach (var entry in _entries)
            {
                assetBinder.BindAsset(entry);
            }
        }
    }

    public sealed class AssetReleaseSchedule<T> : AssetHandleSchedule<T> where T : IAssetBindEntry, IEquatable<T>
    {
        private readonly List<T> _entries = new List<T>();
        
        public AssetReleaseSchedule(T entry)
        {
            _entries.Add(entry);
        }

        public AssetReleaseSchedule(IList<T> entries)
        {
            _entries.AddRange(entries);
        }

        public override bool Precondition(IAssetBinder<T> assetBinder)
        {
            return _entries.All(IsHandleLoaded);
            
            bool IsHandleLoaded(T entry)
            {
                return assetBinder.TryGetBindingHandle(entry, out var handle)
                       && IAssetBinder.CheckHandleLoaded(handle);
            }
        }

        public override void Run(IAssetBinder<T> assetBinder)
        {
            foreach (var entry in _entries)
            {
                assetBinder.ReleaseAsset(entry);
            }
        }
    }
}
