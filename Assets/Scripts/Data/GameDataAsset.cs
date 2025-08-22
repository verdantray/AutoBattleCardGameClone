using System.Collections.Generic;
using ProjectABC.Data.Editor;
using UnityEngine;

namespace ProjectABC.Data
{
    [CreateAssetMenu(fileName = "GameDataAsset", menuName = "Scripting/ScriptableObject Script Menu/DataAssets/GameDataAsset")]
    public class GameDataAsset : DataAsset
    {
        [Header("Game Data")]
        [SerializeField] private List<CardData> cardData;
        [SerializeField] private List<RecruitData> recruitData;
        [SerializeField] private List<WinPointData> winPointData;
        
        public List<CardData> CardData => cardData;
        public List<RecruitData> RecruitData => recruitData;
        public List<WinPointData> WinPointData => winPointData;

#if UNITY_EDITOR
        public override void UpdateDataFromSheet()
        {
            UpdateData(nameof(cardData), cardData);
            UpdateData(nameof(recruitData), recruitData);
            UpdateData(nameof(winPointData), winPointData);
        }
#endif
    }
}
