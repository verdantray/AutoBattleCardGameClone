using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ProjectABC.Engine
{
    public sealed class GlobalFontBinder : MonoBehaviour, IAssetBinder<TMP_FontAsset>
    {
        [SerializeField] private AssetLabelReference basicFontLabel;
        
        private readonly Dictionary<string, AsyncOperationHandle<TMP_FontAsset>> _basicFontHandles = new Dictionary<string, AsyncOperationHandle<TMP_FontAsset>>();
        private readonly Dictionary<string, AsyncOperationHandle<TMP_FontAsset>> _additiveFontHandles = new Dictionary<string, AsyncOperationHandle<TMP_FontAsset>>();

        public async Task BindBasicFontsAsync()
        {
            var locationsHandle = Addressables.LoadResourceLocationsAsync(basicFontLabel);
            await locationsHandle.Task;

            var locations = locationsHandle.Result;
            var tasks = locations
                .Select(location => BindAssetAsync(location, OnBindBasicFontAsset))
                .ToList();
            
            await Task.WhenAll(tasks);
        }

        public Task BindAdditiveFontAsync(string fontName)
        {
            return BindAssetAsync(fontName, OnBindAdditiveFontAsset);
        }
        
        public async Task<TMP_FontAsset> BindAssetAsync(object key, Action<AsyncOperationHandle<TMP_FontAsset>> callback)
        {
            var handle = Addressables.LoadAssetAsync<TMP_FontAsset>(key);
            await handle.Task;
            
            callback?.Invoke(handle);
            
            return handle.Result;
        }

        public void ReleaseAsset(string addressableName)
        {
            if (_additiveFontHandles.TryGetValue(addressableName, out var additiveFontHandle))
            {
                Addressables.Release(additiveFontHandle);
            }
            else if (_basicFontHandles.TryGetValue(addressableName, out var basicFontHandle))
            {
                Addressables.Release(basicFontHandle);
            }
        }

        private void OnBindBasicFontAsset(AsyncOperationHandle<TMP_FontAsset> handle)
        {
            _basicFontHandles.TryAdd(handle.Result.name, handle);
        }

        private void OnBindAdditiveFontAsset(AsyncOperationHandle<TMP_FontAsset> handle)
        {
            TMP_FontAsset additiveFontAsset = handle.Result;
            _additiveFontHandles.TryAdd(handle.Result.name, handle);

            foreach (var basicFontHandle in _basicFontHandles.Values)
            {
                basicFontHandle.Result.fallbackFontAssetTable.Add(additiveFontAsset);
            }
        }
    }
}
