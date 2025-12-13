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

        private readonly Dictionary<string, CardData> _cardIdMap = new Dictionary<string, CardData>();

        private void Initialize(GameDataAsset gameDataAsset)
        {
            CardDataForStarting = new List<CardData>(gameDataAsset.CardDataForStarting);
            CardDataForPiles = new List<CardData>(gameDataAsset.CardDataForPiles);
            RecruitData = new List<RecruitData>(gameDataAsset.RecruitData);
            WinPointData = new List<WinPointData>(gameDataAsset.WinPointData);
            CardEffectData = gameDataAsset.CardEffectData.ToDictionary(data => data.id);

            foreach (var cardData in CardDataForStarting)
            {
                _cardIdMap.TryAdd(cardData.id, cardData);
            }

            foreach (var cardData in CardDataForPiles)
            {
                _cardIdMap.TryAdd(cardData.id, cardData);
            }
        }

        public CardData GetCardData(string cardId)
        {
            return _cardIdMap.GetValueOrDefault(cardId);
        }

        public static Storage CreateInstance(GameDataAsset gameDataAsset)
        {
            Storage instance = CreateInstanceInternal();
            instance.Initialize(gameDataAsset);

            return instance;
        }
    }
}
