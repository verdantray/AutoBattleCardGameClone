using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using ProjectABC.Core;
using ProjectABC.Data;
using ProjectABC.Engine.Scene;
using ProjectABC.Utils;

namespace ProjectABC.Engine
{
    public sealed class SceneLoader : MonoSingleton<SceneLoader>
    {
        [SerializeField] private SceneLoadingProfileAsset persistentWorldProfile;
        [SerializeField] private SceneLoadingProfileAsset[] sceneLoadingProfiles;

        protected override bool SetPersistent => true;

        private SceneLoadingProfileAsset _targetSceneProfileAsset = null;
        private Task _loadingTask = null;

        private void Start()
        {
            // var currentScene = SceneManager.GetActiveScene();
            // if (currentScene.name != GameConst.SceneName.INITIALIZER)
            // {
            //     SceneManager.LoadScene(GameConst.SceneName.INITIALIZER);
            // }
        }

        public Task GetLoadSceneTask(string targetSceneName)
        {
            if (_loadingTask is { IsCompleted: false })
            {
                return _loadingTask;
            }

            _loadingTask = LoadSceneAsync(targetSceneName);
            return _loadingTask;
        }

        private async Task LoadSceneAsync(string targetSceneName)
        {
            if (!persistentWorldProfile.IsLoaded)
            {
                await persistentWorldProfile.LoadSceneAndAssetsAsync(LoadSceneMode.Additive, true);
                await Task.Yield(); // wait 1frame more for awake
                
                PersistentWorldCamera.Instance.BlurTo(1.0f);
            }
            else
            {
                PersistentWorldCamera.Instance.BlurOn(0.25f);
            }

            UnityEngine.SceneManagement.Scene initializerScene = SceneManager.GetSceneByName(GameConst.SceneName.INITIALIZER);
            if (initializerScene.isLoaded)
            {
                await SceneManager.UnloadSceneAsync(initializerScene);
            }
            
            _targetSceneProfileAsset = sceneLoadingProfiles.FirstOrDefault(profile => profile.ProfileName == targetSceneName);
            if (_targetSceneProfileAsset == null)
            {
                Debug.LogError($"Can't find target scene {targetSceneName}");
                PersistentWorldCamera.Instance.BlurOff();
                
                return;
            }
            
            List<Task> loadingSceneTasks = new List<Task>
            {
                Task.Delay(TimeSpan.FromSeconds(0.25)),  // await for blur
                _targetSceneProfileAsset.LoadSceneAndAssetsAsync(),
                // GlobalAssetBinder.Instance.GetAssetBindingTask()
            };

            // Debug.Log("Loading scene and assets");
            await Task.WhenAll(loadingSceneTasks);

            var unloadPreloadedScenesTasks = sceneLoadingProfiles
                .Where(profile => profile.IsLoaded && profile.ProfileName != targetSceneName)
                .Select(profile => profile.UnloadSceneAndAssetsAsync());

            // Debug.Log("unload preloaded scenes");
            await Task.WhenAll(unloadPreloadedScenesTasks);
            await _targetSceneProfileAsset.ActivateSceneAsync();
            
            _targetSceneProfileAsset.GetPostLoadingTask().Forget();
        }

        public Task GetPostSceneLoadingTask()
        {
            return _targetSceneProfileAsset != null
                ? _targetSceneProfileAsset.GetPostLoadingTask()
                : Task.CompletedTask;
        }
    }
}