using System;
using System.Threading.Tasks;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace ProjectABC.Engine
{
    public interface IAssetBinder<T> where T : Object
    {
        public Task<T> BindAssetAsync(object key, Action<AsyncOperationHandle<T>> callback);
        public void ReleaseAsset(string addressableName);
    }
}
