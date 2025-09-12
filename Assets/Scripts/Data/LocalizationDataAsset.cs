using System.Collections.Generic;
using ProjectABC.Data.Editor;
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace ProjectABC.Data
{
    [CreateAssetMenu(fileName = nameof(LocalizationDataAsset), menuName = "Scripting/ScriptableObject Script Menu/DataAssets/LocalizationDataAsset")]
    public class LocalizationDataAsset : DataAsset
    {
        [Header("Localize Data")]
        [SerializeField] private List<LocalizationData> localizationData;
        
        public IReadOnlyList<LocalizationData> LocalizationData => localizationData;

#if UNITY_EDITOR
        public override void UpdateDataFromSheet()
        {
            localizationData.Clear();
            
            UpdateData(nameof(localizationData), localizationData, false);
            
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
#endif
    }
}
