using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProjectABC.Data;


namespace ProjectABC.Core
{
    public record MatchFlowSnapshot
    {
        public readonly IReadOnlyDictionary<IPlayer, MatchSideSnapshot> MatchSideSnapShots;
        
        public MatchFlowSnapshot(params MatchSide[] matchSides)
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
        
        public readonly IReadOnlyList<CardInstance> Deck;
        public readonly IReadOnlyList<CardInstance> Field;
        public readonly InfirmaryInstance Infirmary;
        
        private bool IsAttacking => State == MatchState.Attacking;

        public MatchSideSnapshot(MatchSide matchSide)
        {
            Player = matchSide.Player;
            State = matchSide.State;
            
            Deck = matchSide.Hands.Select(card => new CardInstance(card)).ToList();
            Field = matchSide.Field.Select(card => new CardInstance(card)).ToList();
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

    public record CardInstance
    {
        public string Id => CardData.id;
        public int BasePower => CardData.basePower;
        public ClubType ClubType => CardData.clubType;
        public GradeType GradeType => CardData.gradeType;
        
        public readonly int Power;
        public readonly CardData CardData;
        
        public CardInstance(Card card)
        {
            Power = card.Power;
            CardData = card.CardData;
        }
    }

    public record InfirmaryInstance : IReadOnlyDictionary<string, IReadOnlyList<CardInstance>>
    {
        public readonly IReadOnlyList<string> NameKeyList;
        public readonly IReadOnlyDictionary<string, IReadOnlyList<CardInstance>> CardMap;

        public InfirmaryInstance(IEnumerable<string> nameKeys, IDictionary<string, CardPile> cardMap)
        {
            NameKeyList = new List<string>(nameKeys);
            CardMap = new Dictionary<string, IReadOnlyList<CardInstance>>(cardMap.ToDictionary(KeySelector,
                ValueSelector));
        }

        public IReadOnlyList<CardInstance> this[int index] => CardMap[NameKeyList[index]];

        private static string KeySelector(KeyValuePair<string, CardPile> kvPair)
        {
            return kvPair.Key;
        }

        private static IReadOnlyList<CardInstance> ValueSelector(KeyValuePair<string, CardPile> kvPair)
        {
            return new List<CardInstance>(kvPair.Value.Select(card => new CardInstance(card)));
        }
        
        #region inherits IReadOnlyDictionary

        public IEnumerator<KeyValuePair<string, IReadOnlyList<CardInstance>>> GetEnumerator()
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

        public bool TryGetValue(string key, out IReadOnlyList<CardInstance> value)
        {
            return CardMap.TryGetValue(key, out value);
        }

        public IReadOnlyList<CardInstance> this[string key] => throw new NotImplementedException();

        public IEnumerable<string> Keys => NameKeyList;
        public IEnumerable<IReadOnlyList<CardInstance>> Values => CardMap.Values;

        #endregion
    }
}
