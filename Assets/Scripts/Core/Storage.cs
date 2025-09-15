using System.Collections.Generic;
using System.Linq;
using ProjectABC.Data;
using ProjectABC.Utils;

namespace ProjectABC.Core
{
    public class Storage : Singleton<Storage>
    {
        public IReadOnlyList<CardData> CardDataForStarting;
        public IReadOnlyList<CardData> CardDataForPiles;
        public IReadOnlyList<RecruitData> RecruitData;
        public IReadOnlyList<WinPointData> WinPointData;
        public IReadOnlyDictionary<string, CardEffectData> CardEffectData;

        private void Initialize(GameDataAsset gameDataAsset)
        {
            CardDataForStarting = new List<CardData>(gameDataAsset.CardDataForStarting);
            CardDataForPiles = new List<CardData>(gameDataAsset.CardDataForPiles);
            RecruitData = new List<RecruitData>(gameDataAsset.RecruitData);
            WinPointData = new List<WinPointData>(gameDataAsset.WinPointData);
            CardEffectData = gameDataAsset.CardEffectData.ToDictionary(data => data.id);
        }

        public static Storage CreateInstance(GameDataAsset gameDataAsset)
        {
            Storage instance = CreateInstanceInternal();
            instance.Initialize(gameDataAsset);

            return instance;
        }
    }
}
