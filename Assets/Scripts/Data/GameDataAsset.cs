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
        [Header("Game Data")]
        [SerializeField] private List<CardData> cardDataForStarting;
        [SerializeField] private List<CardData> cardDataForPiles;
        [SerializeField] private List<RecruitData> recruitData;
        [SerializeField] private List<WinPointData> winPointData;

        public List<CardData> CardDataForStarting => cardDataForStarting;
        public List<CardData> CardDataForPiles => cardDataForPiles;
        public List<RecruitData> RecruitData => recruitData;
        public List<WinPointData> WinPointData => winPointData;

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
#endif
    }
}
