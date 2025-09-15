using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProjectABC.Data;


namespace ProjectABC.Core
{
    public record MatchSnapshot
    {
        public readonly IReadOnlyDictionary<IPlayer, MatchSideSnapshot> MatchSideSnapShots;
        
        public MatchSnapshot(GameState gameState, params MatchSide[] matchSides)
        {
            MatchSideSnapShots = new Dictionary<IPlayer, MatchSideSnapshot>
            {
                { matchSides[0].Player, new MatchSideSnapshot(matchSides[0], matchSides[1], gameState) },
                { matchSides[1].Player, new MatchSideSnapshot(matchSides[1], matchSides[0], gameState) },
            };
        }

        public bool IsParticipants(IPlayer player) => MatchSideSnapShots.ContainsKey(player);

        public bool TryGetMatchSideSnapshot(IPlayer player, out MatchSideSnapshot matchSideSnapshot)
        {
            return MatchSideSnapShots.TryGetValue(player, out matchSideSnapshot);
        }
    }
    
    public record MatchSideSnapshot
    {
        public readonly IPlayer Player;
        public readonly MatchState State;
        
        public readonly IReadOnlyList<CardSnapshot> Deck;
        public readonly IReadOnlyList<CardSnapshot> Field;
        public readonly InfirmaryInstance Infirmary;
        
        public bool IsAttacking => State == MatchState.Attacking;
        
        public int EffectivePower
        {
            get
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

        public MatchSideSnapshot(MatchSide ownSide, MatchSide otherSide, GameState gameState)
        {
            CardBuffArgs args = new CardBuffArgs(ownSide, otherSide, gameState);
            
            Player = ownSide.Player;
            State = ownSide.State;
            
            Deck = ownSide.Deck.Select(card => new CardSnapshot(card, args)).ToList();
            Field = ownSide.Field.Select(card => new CardSnapshot(card, args)).ToList();
            Infirmary = ownSide.Infirmary.GetSnapshotInstance(args);
        }
    }

    public record CardSnapshot
    {
        public readonly string Id;
        public readonly ClubType ClubType;
        public readonly GradeType GradeType;
        public readonly int BasePower;
        public readonly Card Card;
        public readonly IReadOnlyList<BuffSnapshot> Buffs;
        
        public int Power => Buffs.Sum(buff => buff.AdditivePower) + BasePower;
        public string Title => Card.Title;
        public string Name => Card.Name;
        public string Description => Card.CardEffect.Description;
        
        public CardSnapshot(Card card, CardBuffArgs args)
        {
            Id = card.Id;
            ClubType = card.ClubType;
            GradeType = card.GradeType;
            BasePower = card.BasePower;
            
            Card = card;

            List<BuffSnapshot> buffSnapshots = new List<BuffSnapshot>();
            CardBuff[] disablerBuffs = card.AppliedCardBuffs
                .Where(buff => buff.Type == BuffType.Disabler && buff.IsBuffActive(card, args))
                .ToArray();

            foreach (var buff in card.AppliedCardBuffs)
            {
                bool isDisabled = !disablerBuffs.Contains(buff)
                                  && disablerBuffs.Any(disabler => disabler.ShouldDisable(buff, card, args));

                BuffSnapshot snapshot = new BuffSnapshot(buff, card, args, isDisabled);
                buffSnapshots.Add(snapshot);
            }
            
            Buffs = buffSnapshots;
        }
    }

    public record BuffSnapshot
    {
        public enum BuffState
        {
            Active,
            InActive,
            Disabled
        }
        
        public readonly BuffType BuffType;
        public readonly BuffState State;
        public readonly int AdditivePower;

        public readonly Card CallCard;
        public string Description => CallCard.CardEffect.Description;

        public BuffSnapshot(CardBuff cardBuff, Card target, CardBuffArgs args, bool isDisabled)
        {
            BuffType = cardBuff.Type;
            State = isDisabled
                ? BuffState.Disabled
                : cardBuff.IsBuffActive(target, args)
                    ? BuffState.Active
                    : BuffState.InActive;
            
            AdditivePower = cardBuff.IsBuffActive(target, args)
                ? cardBuff.CalculateAdditivePower(target, args)
                : 0;
            
            CallCard = cardBuff.CallCard;
        }
    }

    public record InfirmaryInstance : IReadOnlyDictionary<string, IReadOnlyList<CardSnapshot>>
    {
        public readonly IReadOnlyList<string> NameKeyList;
        public readonly IReadOnlyDictionary<string, IReadOnlyList<CardSnapshot>> CardMap;

        public InfirmaryInstance(IEnumerable<string> nameKeys, IDictionary<string, CardPile> cardMap, CardBuffArgs args)
        {
            NameKeyList = new List<string>(nameKeys);
            CardMap = new Dictionary<string, IReadOnlyList<CardSnapshot>>(cardMap.ToDictionary(KeySelector, ValueSelector));
            return;
            
            IReadOnlyList<CardSnapshot> ValueSelector(KeyValuePair<string, CardPile> kvPair)
            {
                return new List<CardSnapshot>(kvPair.Value.Select(card => new CardSnapshot(card, args)));
            }
        }

        public IReadOnlyList<CardSnapshot> this[int index] => CardMap[NameKeyList[index]];

        private static string KeySelector(KeyValuePair<string, CardPile> kvPair)
        {
            return kvPair.Key;
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

        public IReadOnlyList<CardSnapshot> this[string key] => CardMap[key];

        public IEnumerable<string> Keys => NameKeyList;
        public IEnumerable<IReadOnlyList<CardSnapshot>> Values => CardMap.Values;

        #endregion
    }
}
