using UnityEngine;

namespace ProjectABC.Data
{
    [CreateAssetMenu(fileName = nameof(InGameProfileAsset), menuName = "Scripting/ScriptableObject Script Menu/SceneLoadingProfileAsset/InGame")]
    public class InGameProfileAsset : SceneLoadingProfileAsset
    {
        public override string ProfileName => "InGame";
    }
}
