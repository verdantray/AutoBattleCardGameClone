using ProjectABC.Utils;
using UnityEngine;

namespace ProjectABC.Engine
{
    public sealed class GlobalAssetBinder : MonoSingleton<GlobalAssetBinder>
    {
        [SerializeField] private AtlasBinder atlasBinder;
        [SerializeField] private FontBinder fontBinder;
        
        protected override bool SetPersistent => true;
        
        public AtlasBinder AtlasBinder => atlasBinder;
        public FontBinder FontBinder => fontBinder;

        public bool IsAllHandlesLoaded()
        {
            return atlasBinder.IsAllBindingHandlesLoaded() && fontBinder.IsAllBindingHandlesLoaded();
        }
    }
}
