using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectABC.Data;
using ProjectABC.Engine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectABC.Engine
{
    [CreateAssetMenu(fileName = nameof(InGameProfileAsset), menuName = "Scripting/ScriptableObject Script Menu/SceneLoadingProfileAsset/InGame")]
    public sealed class InGameProfileAsset : SceneLoadingProfileAsset
    {
        [SerializeField] private AtlasBindingEntry cardAtlasEntry;
        [SerializeField] private AdditiveFontBindingEntry additiveFontEntry;
        [SerializeField] private AtlasBindingEntry[] atlasEntries;
        [SerializeField] private UIBindingEntry[] uIEntries;
        
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
            
            // TODO : load while enter title or lobby instead of in game
            GlobalAssetBinder.Instance.AtlasBinder.AddAssetBindingEntry(cardAtlasEntry);
            GlobalAssetBinder.Instance.FontBinder.AddAssetBindingEntry(additiveFontEntry);

            foreach (var inGameAtlasEntry in atlasEntries)
            {
                GlobalAssetBinder.Instance.AtlasBinder.AddAssetBindingEntry(inGameAtlasEntry);
            }

            foreach (var inGameUIEntry in uIEntries)
            {
                UIManager.Instance.AddAssetBindingEntry(inGameUIEntry);
            }
            
            await Task.WhenAll(loadingTasks);

            if (!activateOnLoad)
            {
                return;
            }

            await ActivateSceneAsync();
        }

        public override async Task UnloadSceneAndAssetsAsync()
        {
            await base.UnloadSceneAndAssetsAsync();

            foreach (var inGameAtlasEntry in atlasEntries)
            {
                GlobalAssetBinder.Instance.AtlasBinder.RemoveAssetBindingEntry(inGameAtlasEntry);
            }

            foreach (var inGameUIEntry in uIEntries)
            {
                UIManager.Instance.RemoveAssetBindingEntry(inGameUIEntry);
            }
        }
    }
}
