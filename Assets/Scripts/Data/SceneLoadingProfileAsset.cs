using System.Threading.Tasks;
using UnityEngine;

namespace ProjectABC.Data
{
    public enum EScene
    {
        Lobby,
        InGame
    }
    
    public abstract class SceneLoadingProfileAsset : ScriptableObject
    {
        public abstract Task LoadingAssetsAsync();
        
        public virtual void OnSceneLoaded()
        {
            
        }
    }
}
