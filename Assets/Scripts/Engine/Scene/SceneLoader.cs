using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using ProjectABC.Data;
using ProjectABC.Utils;
using UnityEngine.SceneManagement;

namespace ProjectABC.Engine.Scene
{
    public sealed class SceneLoader : MonoSingleton<SceneLoader>
    {
        [SerializeField] private SceneLoadingProfileAsset persistentWorldProfile;
        [SerializeField] private SceneLoadingProfileAsset[] sceneLoadingProfiles;

        public async Task LoadSceneAsync(string targetSceneName)
        {
            if (!persistentWorldProfile.IsLoaded)
            {
                await persistentWorldProfile.LoadSceneAndAssetsAsync(LoadSceneMode.Additive, true);
                await Task.Yield(); // wait 1frame more for awake
                
                // PersistentWorldCamera.Instance.BlurTo(1.0f);
            }
            // else
            {
                PersistentWorldCamera.Instance.BlurOn(0.5f);
            }
            
            SceneLoadingProfileAsset targetSceneProfile = sceneLoadingProfiles.FirstOrDefault(profile => profile.ProfileName == targetSceneName);
            if (targetSceneProfile == null)
            {
                Debug.LogError($"Can't find target scene {targetSceneName}");
                PersistentWorldCamera.Instance.BlurOff();
                
                return;
            }

            List<Task> tasks = new List<Task>
            {
                Task.Delay(TimeSpan.FromSeconds(0.5)),  // await for blur
                targetSceneProfile.LoadSceneAndAssetsAsync(),
            };

            var unloadPreloadedScenesTasks = sceneLoadingProfiles
                .Where(profile => profile.IsLoaded && profile.ProfileName != targetSceneName)
                .Select(profile => profile.UnloadSceneAndAssetsAsync());
            tasks.AddRange(unloadPreloadedScenesTasks);

            await Task.WhenAll(tasks);
            await targetSceneProfile.ActivateSceneAsync();

            targetSceneProfile.GetPostLoadingTask().Forget();
        }
    }
}