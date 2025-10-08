using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace ProjectABC.Data
{
    public abstract class SceneLoadingProfileAsset : ScriptableObject
    {
        [SerializeField] protected AssetReference sceneReference;
        [SerializeField] protected AssetReferenceGameObject[] assetRefsForPreload;
        [SerializeField] protected AssetReferenceGameObject[] assetRefsForPostLoad;

        protected readonly Dictionary<string, AsyncOperationHandle> AssetHandles = new Dictionary<string, AsyncOperationHandle>();
        protected readonly HashSet<string> RegisteredAssetGUIDs = new HashSet<string>();
        
        protected AsyncOperationHandle<SceneInstance> SceneHandle = default;
        
        public abstract string ProfileName { get; }
        public bool IsLoaded => IsSceneLoaded && IsAssetsPreLoaded;
        public bool IsSceneLoaded => IsHandleLoaded(SceneHandle);
        public bool IsAssetsPreLoaded => IsAssetsLoaded(assetRefsForPreload);

        public virtual async Task LoadSceneAndAssetsAsync(LoadSceneMode mode = LoadSceneMode.Additive, bool activateOnLoad = false)
        {
            List<Task> loadingTasks = new List<Task>();
            
            if (!SceneHandle.IsValid())
            {
                SceneHandle = GetLoadingSceneHandle(mode, activateOnLoad);
            }

            if (!SceneHandle.IsDone)
            {
                loadingTasks.Add(SceneHandle.Task);
            }
            
            RegisterLoadingHandles(assetRefsForPreload);
            loadingTasks.AddRange(AssetHandles.Values.Select(handle => handle.Task));
            
            await Task.WhenAll(loadingTasks);

            if (!activateOnLoad)
            {
                return;
            }

            await ActivateSceneAsync();
        }

        public AsyncOperation ActivateSceneAsync()
        {
            if (!IsLoaded)
            {
                Debug.LogError($"{nameof(SceneLoadingProfileAsset)} : scene or assets loading is not completed");
                return new AsyncOperation();
            }

            return SceneHandle.Result.ActivateAsync();
        }

        public Task GetPostLoadingTask()
        {
            var postLoadingAssetGUIDs = assetRefsForPostLoad
                .Select(assetRef => assetRef.AssetGUID)
                .Distinct()
                .ToArray();
            
            RegisterLoadingHandles(assetRefsForPostLoad);
            var tasks = AssetHandles
                .Where(kvPair => postLoadingAssetGUIDs.Contains(kvPair.Key))
                .Select(kvPair => kvPair.Value.Task);
            
            return Task.WhenAll(tasks);
        }

        protected AsyncOperationHandle<SceneInstance> GetLoadingSceneHandle(LoadSceneMode mode, bool activateOnLoad)
        {
            return Addressables.LoadSceneAsync(sceneReference, mode, activateOnLoad);
        }

        protected void RegisterLoadingHandles(AssetReferenceGameObject[] assetReferences)
        {
            foreach (var assetRefOnSceneUsed in assetReferences)
            {
                if (RegisteredAssetGUIDs.Contains(assetRefOnSceneUsed.AssetGUID))
                {
                    Debug.LogWarning($"{nameof(SceneLoadingProfileAsset)} : duplicate asset guid {assetRefOnSceneUsed.AssetGUID}.. check fields");
                    continue;
                }

                string key = assetRefOnSceneUsed.AssetGUID;
                var handle = Addressables.LoadAssetAsync<GameObject>(assetRefOnSceneUsed);

                RegisteredAssetGUIDs.Add(key);
                AssetHandles.Add(key, handle);
            }
        }

        public virtual async Task UnloadSceneAndAssetsAsync()
        {
            if (IsSceneLoaded)
            {
                var unloadSceneHandle = Addressables.UnloadSceneAsync(SceneHandle);
                await unloadSceneHandle.Task;
            }

            if (AssetHandles.Count > 0)
            {
                await Task.WhenAll(AssetHandles.Values.Where(handle => !handle.IsDone).Select(handle => handle.Task));
                UnregisterLoadingHandles();
            }
        }

        protected void UnregisterLoadingHandles()
        {
            foreach (var handle in AssetHandles.Values.Where(handle => handle.IsValid()))
            {
                Addressables.Release(handle);
            }
            
            RegisteredAssetGUIDs.Clear();
            AssetHandles.Clear();
        }

        private static bool IsHandleLoaded(AsyncOperationHandle handle)
        {
            return handle.IsValid() && handle.Status == AsyncOperationStatus.Succeeded;
        }
        
        private bool IsAssetsLoaded(params AssetReferenceGameObject[] assetReferences)
        {
            var assetGuids = assetReferences
                .Select(assetRef => assetRef.AssetGUID)
                .Distinct()
                .ToArray();
            
            return RegisteredAssetGUIDs.IsSupersetOf(assetGuids)
                   && AssetHandles
                       .Where(kvPair => assetGuids.Contains(kvPair.Key))
                       .All(kvPair => IsHandleLoaded(kvPair.Value));
        }
    }
}
