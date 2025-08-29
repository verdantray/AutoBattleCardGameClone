using System.Collections.Generic;
using ProjectABC.Data;

namespace ProjectABC.Core
{
    public class Storage
    {
        public static Storage Instance { get; private set; }
        public static bool HasInstance => Instance != null;

        public readonly List<CardData> CardDataForStarting = new List<CardData>();
        public readonly List<CardData> CardDataForPiles = new List<CardData>();
        public readonly List<RecruitData> RecruitData = new List<RecruitData>();
        public readonly List<WinPointData> WinPointData = new List<WinPointData>();

        public static void CreateInstance(GameDataAsset gameDataAsset)
        {
            Instance ??= new Storage();
            
            Instance.CardDataForStarting.Clear();
            Instance.CardDataForStarting.AddRange(gameDataAsset.CardDataForStarting);
            
            Instance.CardDataForPiles.Clear();
            Instance.CardDataForPiles.AddRange(gameDataAsset.CardDataForPiles);
            
            Instance.RecruitData.Clear();
            Instance.RecruitData.AddRange(gameDataAsset.RecruitData);
            
            Instance.WinPointData.Clear();
            Instance.WinPointData.AddRange(gameDataAsset.WinPointData);
        }
    }
}
