using ProjectABC.Data;
using UnityEngine;

namespace ProjectABC.Engine
{
    [CreateAssetMenu(fileName = nameof(PersistentWorldProfileAsset), menuName = "Scripting/ScriptableObject Script Menu/SceneLoadingProfileAsset/PersistentWorld")]
    public class PersistentWorldProfileAsset : SceneLoadingProfileAsset
    {
        public override string ProfileName => "PersistentWorld";
    }
}
