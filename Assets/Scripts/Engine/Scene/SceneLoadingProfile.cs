using System.Threading.Tasks;
using UnityEngine;


namespace ProjectABC.Engine.Scene
{
    public enum SceneType
    {
        Title,
        InGame,
    }
    
    public abstract class SceneLoadingProfile : ScriptableObject
    {
        public virtual SceneType SceneType { get; }
        
        public abstract Task LoadAsync();
    }
}
