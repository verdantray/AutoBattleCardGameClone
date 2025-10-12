using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectABC.Data
{
    [CreateAssetMenu(fileName = nameof(InGameProfileAsset), menuName = "Scripting/ScriptableObject Script Menu/SceneLoadingProfileAsset/InGame")]
    public sealed class InGameProfileAsset : SceneLoadingProfileAsset
    {
        [SerializeField] private AtlasBindingEntry[] sceneAtlasEntries;
        
        public override string ProfileName => "InGame";

        public override async Task LoadSceneAndAssetsAsync(LoadSceneMode mode = LoadSceneMode.Additive, bool activateOnLoad = false)
        {
            List<Task> loadingTasks = new List<Task>();
            
            if (!SceneHandle.IsValid())
            {
                SceneHandle = LoadSceneAsync(mode, activateOnLoad);
            }

            if (!SceneHandle.IsDone)
            {
                loadingTasks.Add(SceneHandle.Task);
            }
            
            loadingTasks.AddRange(assetRefsForPreload.Select(GetAssetLoadingTask));
            
            await Task.WhenAll(loadingTasks);

            if (!activateOnLoad)
            {
                return;
            }

            await ActivateSceneAsync();
        }
    }
}
