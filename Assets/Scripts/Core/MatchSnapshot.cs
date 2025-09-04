using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProjectABC.Data;


namespace ProjectABC.Core
{
    public record MatchSnapshot
    {
        public readonly IReadOnlyDictionary<IPlayer, MatchSideSnapshot> MatchSideSnapShots;
        
        public MatchSnapshot(params MatchSide[] matchSides)
        {
            MatchSideSnapShots =
                new Dictionary<IPlayer, MatchSideSnapshot>(matchSides.ToDictionary(KeySelector, ValueSelector));
        }

        public bool IsParticipants(IPlayer player) => MatchSideSnapShots.ContainsKey(player);

        public bool TryGetMatchSideSnapshot(IPlayer player, out MatchSideSnapshot matchSideSnapshot)
        {
            return MatchSideSnapShots.TryGetValue(player, out matchSideSnapshot);
        }
        
        private static IPlayer KeySelector(MatchSide matchSide) => matchSide.Player;
        private static MatchSideSnapshot ValueSelector(MatchSide matchSide) => new MatchSideSnapshot(matchSide);
    }
    
    public record MatchSideSnapshot
    {
        public readonly IPlayer Player;
        public readonly MatchState State;
        
        public readonly IReadOnlyList<CardSnapshot> Deck;
        public readonly IReadOnlyList<CardSnapshot> Field;
        public readonly InfirmaryInstance Infirmary;
        
        private bool IsAttacking => State == MatchState.Attacking;

        public MatchSideSnapshot(MatchSide matchSide)
        {
            Player = matchSide.Player;
            State = matchSide.State;
            
            Deck = matchSide.Deck.Select(card => new CardSnapshot(card)).ToList();
            Field = matchSide.Field.Select(card => new CardSnapshot(card)).ToList();
            Infirmary = matchSide.Infirmary.GetSnapshotInstance;
        }

        public int GetEffectivePower()
        {
            if (Field.Count == 0)
            {
                return 0;
            }

            int effectivePower = IsAttacking
                ? Field.Sum(card => card.Power)
                : Field[^1].Power;
            
            return effectivePower;
        }
    }

    public record CardSnapshot
    {
        public string Id => CardData.id;
        public int BasePower => CardData.basePower;
        public ClubType ClubType => CardData.clubType;
        public GradeType GradeType => CardData.gradeType;
        
        public readonly int Power;
        public readonly CardData CardData;
        
        public CardSnapshot(Card card)
        {
            // TODO : apply buff either
            Power = card.BasePower;
            CardData = card.CardData;
        }
    }

    public record InfirmaryInstance : IReadOnlyDictionary<string, IReadOnlyList<CardSnapshot>>
    {
        public readonly IReadOnlyList<string> NameKeyList;
        public readonly IReadOnlyDictionary<string, IReadOnlyList<CardSnapshot>> CardMap;

        public InfirmaryInstance(IEnumerable<string> nameKeys, IDictionary<string, CardPile> cardMap)
        {
            NameKeyList = new List<string>(nameKeys);
            CardMap = new Dictionary<string, IReadOnlyList<CardSnapshot>>(cardMap.ToDictionary(KeySelector,
                ValueSelector));
        }

        public IReadOnlyList<CardSnapshot> this[int index] => CardMap[NameKeyList[index]];

        private static string KeySelector(KeyValuePair<string, CardPile> kvPair)
        {
            return kvPair.Key;
        }

        private static IReadOnlyList<CardSnapshot> ValueSelector(KeyValuePair<string, CardPile> kvPair)
        {
            return new List<CardSnapshot>(kvPair.Value.Select(card => new CardSnapshot(card)));
        }
        
        #region inherits IReadOnlyDictionary

        public IEnumerator<KeyValuePair<string, IReadOnlyList<CardSnapshot>>> GetEnumerator()
        {
            return CardMap.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return CardMap.GetEnumerator();
        }

        public int Count => CardMap.Count;
        public bool ContainsKey(string key)
        {
            return CardMap.ContainsKey(key);
        }

        public bool TryGetValue(string key, out IReadOnlyList<CardSnapshot> value)
        {
            return CardMap.TryGetValue(key, out value);
        }

        public IReadOnlyList<CardSnapshot> this[string key] => throw new NotImplementedException();

        public IEnumerable<string> Keys => NameKeyList;
        public IEnumerable<IReadOnlyList<CardSnapshot>> Values => CardMap.Values;

        #endregion
    }
}
