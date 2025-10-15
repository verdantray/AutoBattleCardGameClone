using System.Threading.Tasks;
using ProjectABC.Utils;
using UnityEngine;

namespace ProjectABC.Data
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

        public Task GetAssetBindingTask()
        {
            return Task.WhenAll(atlasBinder.GetAssetLoadingTask(), fontBinder.GetAssetLoadingTask());
        }
    }
}
