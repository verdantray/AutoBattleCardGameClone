using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProjectABC.Data;


namespace ProjectABC.Core
{
    public class MatchSnapshot
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
    
    public class MatchSideSnapshot
    {
        public readonly IPlayer Player;
        public readonly int Score;
        public readonly MatchPosition Position;
        
        public readonly IReadOnlyList<CardReference> Deck;
        public readonly IReadOnlyList<CardReference> Field;
        public readonly InfirmaryInstance Infirmary;
        
        public bool IsAttacking => Position == MatchPosition.Attacking;

        public MatchSideSnapshot(MatchSide ownSide, MatchSide otherSide, GameState gameState)
        {
            CardBuffArgs args = new CardBuffArgs(ownSide, otherSide, gameState);
            
            Player = ownSide.Player;
            Score = gameState.ScoreBoard.GetTotalWinPoints(Player);
            Position = ownSide.Position;

            Deck = ownSide.Deck.Select(card => new CardReference(card, args)).ToList();
            Field = ownSide.Field.Select(card => new CardReference(card, args)).ToList();
            Infirmary = ownSide.Infirmary.GetSnapshotInstance(args);
        }
    }

    public record CardReference
    {
        public readonly string CardId;
        public readonly CardLocation CardLocation;
        public readonly IReadOnlyList<BuffSnapshot> Buffs;

        public CardData CardData => Storage.Instance.GetCardData(CardId);

        // constructor for only use card data
        public CardReference(string cardId)
        {
            CardId = cardId;
            CardLocation = null;
            Buffs = new List<BuffSnapshot>();
        }
        
        public CardReference(Card card, CardBuffArgs args)
        {
            CardId = card.Id;
            
            card.TryGetCardLocation(args.OwnSide, out CardLocation);

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
    
    public enum BuffState
    {
        Active,         // active buff effect
        Inactive,       // can't active effect because of no met condition
        Disabled,       // can't active effect because of other buff effect is active has disabler type
    }

    public record BuffSnapshot
    {
        public readonly BuffType Type;
        public readonly BuffState State;
        public readonly int AdditivePower;
        public readonly string Description;
        public readonly CardLocation CallCardLocation;

        public BuffSnapshot(CardBuff buff, Card target, CardBuffArgs args, bool isDisabled)
        {
            bool isBuffActive = buff.IsBuffActive(target, args);
            
            Type = buff.Type;
            State = isDisabled
                ? BuffState.Disabled
                : isBuffActive
                    ? BuffState.Active
                    : BuffState.Inactive;

            AdditivePower = isBuffActive
                ? buff.CalculateAdditivePower(target, args)
                : 0;
            
            Description = buff.CallCard.CardEffect.Description;

            bool isOwnSide = args.OwnSide.Player == buff.CallCard.Owner;
            buff.CallCard.TryGetCardLocation(isOwnSide ? args.OwnSide : args.OtherSide, out CallCardLocation);
        }
    }

    public record InfirmaryInstance : IReadOnlyDictionary<string, IReadOnlyList<CardReference>>
    {
        public readonly IReadOnlyList<string> NameKeyList;
        public readonly IReadOnlyDictionary<string, IReadOnlyList<CardReference>> CardMap;

        public InfirmaryInstance(IEnumerable<string> nameKeys, IDictionary<string, CardPile> cardMap, CardBuffArgs args)
        {
            NameKeyList = new List<string>(nameKeys);
            CardMap = new Dictionary<string, IReadOnlyList<CardReference>>(cardMap.ToDictionary(KeySelector, ValueSelector));
            return;
            
            IReadOnlyList<CardReference> ValueSelector(KeyValuePair<string, CardPile> kvPair)
            {
                return kvPair.Value.Select(card => new CardReference(card, args)).ToList();
            }
        }

        public IReadOnlyList<CardReference> this[int index] => CardMap[NameKeyList[index]];

        private static string KeySelector(KeyValuePair<string, CardPile> kvPair)
        {
            return kvPair.Key;
        }
        
        #region inherits IReadOnlyDictionary

        public IEnumerator<KeyValuePair<string, IReadOnlyList<CardReference>>> GetEnumerator()
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

        public bool TryGetValue(string key, out IReadOnlyList<CardReference> value)
        {
            return CardMap.TryGetValue(key, out value);
        }

        public IReadOnlyList<CardReference> this[string key] => CardMap[key];

        public IEnumerable<string> Keys => NameKeyList;
        public IEnumerable<IReadOnlyList<CardReference>> Values => CardMap.Values;

        #endregion
    }
}
