using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectABC.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;

namespace ProjectABC.Engine
{
    public enum AtlasQuality
    {
        None,
        Low,
        Medium,
        High,
    }
    
    public record AtlasIdentifier
    {
        private const char IDENTIFIER_SEPARATOR = '_';
        
        public string AtlasName;
        public AtlasQuality Quality;

        public AtlasIdentifier(string atlasIdentifier)
        {
            string[] split = atlasIdentifier.Split(IDENTIFIER_SEPARATOR);
            AtlasName = split[0];
            Quality = (AtlasQuality)Enum.Parse(typeof(AtlasQuality), split[1], true);
        }

        public string GetAddressableName()
        {
            return $"{AtlasName}{IDENTIFIER_SEPARATOR}{Quality.ToString().ToLowerInvariant()}";
        }

        public static bool IsVariants(AtlasIdentifier a, AtlasIdentifier b)
        {
            return a != null && b != null && a.AtlasName == b.AtlasName;
        }
    }
    
    public sealed class GlobalAtlasBinder : MonoBehaviour, IAssetBinder<SpriteAtlas>
    {
        private readonly Dictionary<AtlasIdentifier, AsyncOperationHandle<SpriteAtlas>> _atlasHandles = new Dictionary<AtlasIdentifier, AsyncOperationHandle<SpriteAtlas>>();

        private void OnEnable()
        {
            SpriteAtlasManager.atlasRequested -= OnAtlasRequested;
            SpriteAtlasManager.atlasRequested += OnAtlasRequested;
        }

        private void OnDisable()
        {
            SpriteAtlasManager.atlasRequested -= OnAtlasRequested;
        }

        public async Task<SpriteAtlas> BindAssetAsync(object key, Action<AsyncOperationHandle<SpriteAtlas>> callback)
        {
            string stringIdentifier = key.ToString();
            AtlasIdentifier toBindId = new AtlasIdentifier(stringIdentifier);

            AtlasIdentifier preBindIdentifier = _atlasHandles.Keys.FirstOrDefault(id => id == toBindId || AtlasIdentifier.IsVariants(id, toBindId));
            AsyncOperationHandle preBindHandle = default;
            
            if (preBindIdentifier != null)
            {
                preBindHandle = _atlasHandles[preBindIdentifier];
                _atlasHandles.Remove(preBindIdentifier);
            }

            AsyncOperationHandle<SpriteAtlas> toBindHandle = Addressables.LoadAssetAsync<SpriteAtlas>(stringIdentifier);
            _atlasHandles.Add(toBindId, toBindHandle);

            await toBindHandle.Task;

            if (preBindHandle.IsValid())
            {
                if (!preBindHandle.IsDone)
                {
                    await preBindHandle.Task;
                }
                
                Addressables.Release(preBindIdentifier);
            }
            
            callback?.Invoke(toBindHandle);
            return toBindHandle.Result;
        }

        public void ReleaseAsset(string addressableName)
        {
            AtlasIdentifier toRelease =  new AtlasIdentifier(addressableName);
            if (!_atlasHandles.TryGetValue(toRelease, out var handle))
            {
                Debug.LogWarning($"{nameof(GlobalAtlasBinder)} : {addressableName} not bind, can't release");
                return;
            }
            
            Addressables.Release(handle);
        }

        private void OnAtlasRequested(string addressableName, Action<SpriteAtlas> callback)
        {
            AtlasIdentifier requested =  new AtlasIdentifier(addressableName);
            if (_atlasHandles.TryGetValue(requested, out var atlasHandle) && atlasHandle.IsValid())
            {
                if (atlasHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    callback.Invoke(atlasHandle.Result);
                }
                else
                {
                    atlasHandle.Completed += handle => callback?.Invoke(handle.Result);
                }
                
                return;
            }
            
            BindAssetAsync(addressableName, handle => callback?.Invoke(handle.Result)).Forget();
        }
    }
}
