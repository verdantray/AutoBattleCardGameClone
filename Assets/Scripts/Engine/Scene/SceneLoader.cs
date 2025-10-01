using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using ProjectABC.Core;
using ProjectABC.Utils;

namespace ProjectABC.Engine.Scene
{
    public sealed class SceneLoader : MonoSingleton<SceneLoader>
    {
        private void Start()
        {
            
        }

        public async Task LoadSceneAsync(string targetSceneName)
        {
            #region 1. Open and active loading scene

            if (!IsSceneLoaded(GameConst.SceneName.LOADING))
            {
                var loadingSceneHandle = SceneManager.LoadSceneAsync(GameConst.SceneName.LOADING, LoadSceneMode.Additive);
                loadingSceneHandle!.allowSceneActivation = true;

                await loadingSceneHandle;
            }

            var loadingScene = SceneManager.GetSceneByName(GameConst.SceneName.LOADING);
            SceneManager.SetActiveScene(loadingScene);

            #endregion

            #region 2. Unload unuse scenes

            List<Task> unloadingTasks = new List<Task>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                bool isUnloadable = scene.name != GameConst.SceneName.LOADING
                                    && scene.name != GameConst.SceneName.PERSISTENT_WORLD
                                    && scene.name != targetSceneName;

                if (!isUnloadable)
                {
                    continue;
                }

                unloadingTasks.Add(GetSceneUnloadingTask(scene.name));
            }
            
            await Task.WhenAll(unloadingTasks);

            #endregion

            #region 3. Load persistent word scene if not loaded

            Task persistentWorldLoadingTask = Task.CompletedTask;
            if (!IsSceneLoaded(GameConst.SceneName.PERSISTENT_WORLD))
            {
                persistentWorldLoadingTask = GetSceneLoadingTask(GameConst.SceneName.PERSISTENT_WORLD);
            }

            #endregion
            
            // 4. load target scene and wait until loading complete
            Task targetSceneLoadingTask = GetSceneLoadingTask(targetSceneName);
            
            await Task.WhenAll(persistentWorldLoadingTask, targetSceneLoadingTask);

            var target = SceneManager.GetSceneByName(targetSceneName);
            if (target.IsValid() && target.isLoaded)
            {
                SceneManager.SetActiveScene(target);
            }

            // 5. unload and close loading scene
            await SceneManager.UnloadSceneAsync(loadingScene);
        }

        private static bool IsSceneLoaded(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                return false;
            }

            var scene = SceneManager.GetSceneByName(sceneName);
            return scene.IsValid() && scene.isLoaded;
        }

        private static async Task GetSceneLoadingTask(string sceneName)
        {
            var asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            asyncOperation!.allowSceneActivation = true;
            
            await asyncOperation;
        }

        private static async Task GetSceneUnloadingTask(string sceneName)
        {
            await SceneManager.UnloadSceneAsync(sceneName);
        }
    }
}