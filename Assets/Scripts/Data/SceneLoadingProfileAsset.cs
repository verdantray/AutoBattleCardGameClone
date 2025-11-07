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
        
        public abstract string ProfileName { get; }
        public bool IsLoaded => IsSceneLoaded && IsAssetsPreLoaded;
        public bool IsSceneLoaded => IsHandleLoaded(SceneHandle);
        public bool IsAssetsPreLoaded => IsAssetsLoaded(assetRefsForPreload);

        protected AsyncOperationHandle<SceneInstance> SceneHandle = default;
        protected readonly Dictionary<string, AsyncOperationHandle> AssetHandles = new Dictionary<string, AsyncOperationHandle>();

        public virtual async Task LoadSceneAndAssetsAsync(LoadSceneMode mode = LoadSceneMode.Additive, bool activateOnLoad = false)
        {
            List<Task> loadingTasks = new List<Task>();
            
            if (!SceneHandle.IsValid())
            {
                 SceneHandle = LoadSceneAsync(mode, activateOnLoad);
                 // Debug.Log($"load scene / {SceneHandle.IsValid()}");
            }

            if (!SceneHandle.IsDone)
            {
                loadingTasks.Add(SceneHandle.Task);
            }
            
            loadingTasks.AddRange(assetRefsForPreload.Select(GetAssetLoadingTask));
            
            await Task.WhenAll(loadingTasks);
            
            // Debug.Log($"Loaded : {IsLoaded} / SceneLoaded : {IsSceneLoaded} / Preloaded : {IsAssetsPreLoaded}");
            
            if (!activateOnLoad)
            {
                return;
            }

            await ActivateSceneAsync();
        }

        public virtual AsyncOperation ActivateSceneAsync()
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
            return Task.WhenAll(assetRefsForPostLoad.Select(GetAssetLoadingTask));
        }

        protected AsyncOperationHandle<SceneInstance> LoadSceneAsync(LoadSceneMode mode, bool activateOnLoad)
        {
            return Addressables.LoadSceneAsync(sceneReference, mode, activateOnLoad);
        }

        protected Task GetAssetLoadingTask<TObject>(AssetReferenceT<TObject> assetReference) where TObject : Object
        {
            if (!AssetHandles.TryGetValue(assetReference.AssetGUID, out var handle) || !handle.IsValid())
            {
                handle =  Addressables.LoadAssetAsync<TObject>(assetReference);
                AssetHandles.Add(assetReference.AssetGUID, handle);
            }

            return handle.Task;
        }

        public virtual async Task UnloadSceneAndAssetsAsync()
        {
            if (IsSceneLoaded)
            {
                var unloadSceneHandle = Addressables.UnloadSceneAsync(SceneHandle);
                await unloadSceneHandle.Task;
            }

            List<Task> tasksLoadingYet = new List<Task>();
            
            tasksLoadingYet.AddRange(assetRefsForPreload.Where(assetRef => assetRef.IsValid()).Select(GetAssetLoadingTask));
            tasksLoadingYet.AddRange(assetRefsForPostLoad.Where(assetRef => assetRef.IsValid()).Select(GetAssetLoadingTask));
            
            await Task.WhenAll(tasksLoadingYet);

            ReleaseSceneAssets(assetRefsForPreload);
            ReleaseSceneAssets(assetRefsForPostLoad);
        }

        protected void ReleaseSceneAssets<T>(params T[] assetReferences) where T : AssetReference
        {
            foreach (var assetRef in assetReferences)
            {
                ReleaseSceneAsset(assetRef);
            }
        }

        protected void ReleaseSceneAsset(AssetReference assetRef)
        {
            if (!assetRef.IsValid())
            {
                return;
            }
            
            Addressables.Release(assetRef);
        }

        private static bool IsHandleLoaded(AsyncOperationHandle handle)
        {
            return handle.IsValid() && handle.Status == AsyncOperationStatus.Succeeded;
        }
        
        private bool IsAssetsLoaded(params AssetReferenceGameObject[] assetReferences)
        {
            return assetReferences.All(assetRef => AssetHandles.TryGetValue(assetRef.AssetGUID, out var handle) && IsHandleLoaded(handle));
        }
    }
}
