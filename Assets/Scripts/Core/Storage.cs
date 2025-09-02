using System.Collections.Generic;
using ProjectABC.Data;

namespace ProjectABC.Core
{
    public class Storage
    {
        public static Storage Instance { get; private set; }
        public static bool HasInstance => Instance != null;

        public readonly IReadOnlyList<CardData> CardDataForStarting;
        public readonly IReadOnlyList<CardData> CardDataForPiles;
        public readonly IReadOnlyList<RecruitData> RecruitData;
        public readonly IReadOnlyList<WinPointData> WinPointData;

        private Storage(GameDataAsset gameDataAsset)
        {
            CardDataForStarting = new List<CardData>(gameDataAsset.CardDataForStarting);
            CardDataForPiles = new List<CardData>(gameDataAsset.CardDataForPiles);
            RecruitData = new List<RecruitData>(gameDataAsset.RecruitData);
            WinPointData = new List<WinPointData>(gameDataAsset.WinPointData);
        }

        public static Storage CreateInstance(GameDataAsset gameDataAsset)
        {
            if (!HasInstance)
            {
                Instance = new Storage(gameDataAsset);
            }
            
            return Instance;
        }
    }
}
