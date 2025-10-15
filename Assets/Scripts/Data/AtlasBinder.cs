using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;

namespace ProjectABC.Data
{
    public sealed class AtlasBinder : MonoBehaviour, IAssetBinder<AtlasBindingEntry>
    {
        private readonly List<AtlasBindingEntry> _atlasBindingEntries = new List<AtlasBindingEntry>();
        private readonly Dictionary<AtlasIdentifier, AsyncOperationHandle<SpriteAtlas>> _atlasHandles = new Dictionary<AtlasIdentifier, AsyncOperationHandle<SpriteAtlas>>();

        private readonly Queue<AssetHandleSchedule<AtlasBindingEntry>> _schedules = new Queue<AssetHandleSchedule<AtlasBindingEntry>>();
        
        // TODO : get atlas quality from user setting
        public AtlasQuality CurrentQualitySetting { get; } = AtlasQuality.Medium;
        
        private void OnEnable()
        {
            SpriteAtlasManager.atlasRequested += OnAtlasRequested;
        }

        private void OnDisable()
        {
            SpriteAtlasManager.atlasRequested -= OnAtlasRequested;
        }
        
        // TODO : subscribe user setting change event
        private void OnQualitySettingChanged(AtlasQuality quality)
        {
            AssetBindSchedule<AtlasBindingEntry> rebindSchedule = new AssetBindSchedule<AtlasBindingEntry>(_atlasBindingEntries);
            _schedules.Enqueue(rebindSchedule);
        }

        private void Update()
        {
            if (_schedules.Count == 0)
            {
                return;
            }

            if (!_schedules.Peek().Precondition(this))
            {
                return;
            }
            
            var schedule = _schedules.Dequeue();
            schedule.Run(this);
        }

        public Task GetAssetLoadingTask()
        {
            var tasks = _atlasHandles.Values
                .Where(handle => handle.IsValid() && !handle.IsDone)
                .Select(handle => handle.Task);
            
            return Task.WhenAll(tasks);
        }

        public bool IsAllBindingHandlesLoaded()
        {
            return _atlasHandles.Values.All(handle => IAssetBinder.CheckHandleLoaded(handle));
        }

        public void AddAssetBindingEntry(AtlasBindingEntry entry)
        {
            _atlasBindingEntries.Add(entry);
            
            AssetBindSchedule<AtlasBindingEntry> bindSchedule = new AssetBindSchedule<AtlasBindingEntry>(entry);
            _schedules.Enqueue(bindSchedule);
        }

        public bool RemoveAssetBindingEntry(AtlasBindingEntry entry)
        {
            AssetReleaseSchedule<AtlasBindingEntry> releaseSchedule = new AssetReleaseSchedule<AtlasBindingEntry>(entry);
            _schedules.Enqueue(releaseSchedule);
            
            return _atlasBindingEntries.Remove(entry);
        }

        public bool TryGetBindingHandle(AtlasBindingEntry entry, out AsyncOperationHandle handle)
        {
            var identifier = entry.GetIdentifier(CurrentQualitySetting);
            bool result = _atlasHandles.TryGetValue(identifier, out var handleT);
            
            handle = handleT;
            return result;
        }

        public void BindAsset(AtlasBindingEntry entry)
        {
            AtlasIdentifier identifier = entry.GetIdentifier(CurrentQualitySetting);
            if (_atlasHandles.ContainsKey(identifier))
            {
                Debug.LogWarning($"{nameof(AtlasBinder)} : {identifier.GetAddressableName()} is already bind");
                return;
            }

            string addressableName = identifier.GetAddressableName();
            var handle = Addressables.LoadAssetAsync<SpriteAtlas>(addressableName);
                
            handle.Completed += ReleaseVariantOnCompleted;
            _atlasHandles.Add(identifier, handle);
        }

        public void ReleaseAsset(AtlasBindingEntry entry)
        {
            ReleaseAsset(entry.GetIdentifier(CurrentQualitySetting));
        }

        private void ReleaseAsset(AtlasIdentifier identifier)
        {
            if (!_atlasHandles.TryGetValue(identifier, out var handle) || !handle.IsValid())
            {
                Debug.LogWarning($"{nameof(AtlasBinder)} : {identifier.GetAddressableName()} not bind, can't release");
                return;
            }

            if (!handle.IsDone)
            {
                Debug.LogWarning($"{nameof(AtlasBinder)} : {identifier.GetAddressableName()} asset is loading yet...");
                return;
            }

            _atlasHandles.Remove(identifier);
            handle.Release();
        }

        private void ReleaseVariantOnCompleted(AsyncOperationHandle<SpriteAtlas> handle)
        {
           string addressableName = handle.Result.name;
           AtlasIdentifier identifier = new AtlasIdentifier(addressableName);

           var variantKey = _atlasHandles.Keys.FirstOrDefault(CheckVariant);
           if (variantKey == null)
           {
               return;
           }
           
           ReleaseAsset(variantKey);
           return;

           bool CheckVariant(AtlasIdentifier other)
           {
               return AtlasIdentifier.IsVariants(identifier, other);
           }
        }

        private void OnAtlasRequested(string addressableName, Action<SpriteAtlas> callback)
        {
            AtlasIdentifier requested = new AtlasIdentifier(addressableName);
            if (_atlasHandles.TryGetValue(requested, out var atlasHandle) && atlasHandle.IsValid())
            {
                if (atlasHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    OnLoadHandleCompleted(atlasHandle);
                }
                else if (!atlasHandle.IsDone)
                {
                    atlasHandle.Completed += OnLoadHandleCompleted;
                }
                
                return;
            }

            var handle = Addressables.LoadAssetAsync<SpriteAtlas>(addressableName);
            handle.Completed += ReleaseVariantOnCompleted;
            handle.Completed += OnLoadHandleCompleted;
            
            _atlasHandles.Add(requested, handle);
            return;
            
            void OnLoadHandleCompleted(AsyncOperationHandle<SpriteAtlas> completeHandle)
            {
                callback?.Invoke(completeHandle.Result);
            }
        }
    }
}
