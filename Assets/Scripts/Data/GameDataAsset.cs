using System.Collections.Generic;
using ProjectABC.Data.Editor;
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace ProjectABC.Data
{
    [CreateAssetMenu(fileName = "GameDataAsset", menuName = "Scripting/ScriptableObject Script Menu/DataAssets/GameDataAsset")]
    public class GameDataAsset : DataAsset
    {
        [SerializeField] private List<LocalDataUpdater> localDataUpdaters;
        
        [Header("Game Data")]
        [SerializeField] private List<CardData> cardDataForStarting;
        [SerializeField] private List<CardData> cardDataForPiles;
        [SerializeField] private List<RecruitData> recruitData;
        [SerializeField] private List<WinPointData> winPointData;
        [SerializeField] private List<CardEffectData> cardEffectData;

        public IReadOnlyList<CardData> CardDataForStarting => cardDataForStarting;
        public IReadOnlyList<CardData> CardDataForPiles => cardDataForPiles;
        public IReadOnlyList<RecruitData> RecruitData => recruitData;
        public IReadOnlyList<WinPointData> WinPointData => winPointData;
        public IReadOnlyList<CardEffectData> CardEffectData => cardEffectData;

#if UNITY_EDITOR
        public override void UpdateDataFromSheet()
        {
            UpdateData(nameof(cardDataForStarting), cardDataForStarting);
            UpdateData(nameof(cardDataForPiles), cardDataForPiles);
            UpdateData(nameof(recruitData), recruitData);
            UpdateData(nameof(winPointData), winPointData);
            
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public void UpdateDataFromAssets()
        {
            UpdateLocalData(nameof(cardEffectData), cardEffectData);
            
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void UpdateLocalData<T>(string fieldName, ICollection<T> collection) where T : ILocalFieldUpdatable, new()
        {
            foreach (var localUpdater in localDataUpdaters)
            {
                if (localUpdater.TryUpdateData(fieldName, collection))
                {
                    break;
                }
            }
        }
#endif
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(GameDataAsset))]
    public class GameDataAssetEditor : DataAssetEditor
    {
        private static bool _updateFromAssetFoldOut = true;
        
        protected override string[] PropertyNames => new[] { "sheetAddress", "dataUpdaters", "localDataUpdaters" };
        
        protected override void DrawRemains()
        {
            base.DrawRemains();

            GUILayout.Space(EditorGUIUtility.singleLineHeight * 0.5f);
            
            _updateFromAssetFoldOut = EditorGUILayout.Foldout(_updateFromAssetFoldOut, "Update Data from Assets");
            if (_updateFromAssetFoldOut)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(SerializedProperties[2]);
                GUILayout.Space(EditorGUIUtility.singleLineHeight);

                if (GUILayout.Button("Update data from data assets"))
                {
                    ((GameDataAsset)target).UpdateDataFromAssets();
                }
                
                EditorGUI.indentLevel--;
            }
        }
    }
#endif
}
