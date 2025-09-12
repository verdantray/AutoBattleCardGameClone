using System.Collections.Generic;
using System.Linq;
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
        public readonly IReadOnlyDictionary<string, CardEffectData> CardEffectData;

        private Storage(GameDataAsset gameDataAsset)
        {
            CardDataForStarting = new List<CardData>(gameDataAsset.CardDataForStarting);
            CardDataForPiles = new List<CardData>(gameDataAsset.CardDataForPiles);
            RecruitData = new List<RecruitData>(gameDataAsset.RecruitData);
            WinPointData = new List<WinPointData>(gameDataAsset.WinPointData);
            CardEffectData = gameDataAsset.CardEffectData.ToDictionary(data => data.id, data => data);
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
