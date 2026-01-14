using System;
using System.Linq;
using ProjectABC.Data;

namespace ProjectABC.Core
{
    /// <summary>
    /// 양호실에 있는 특정 소속 카드 중 무작위 n장을 덱 맨 아래로 넣음
    /// </summary>
    public sealed class MoveRandomCardsBelongClubsFromInfirmaryToDeck : CardEffect
    {
        private readonly int _cardsAmount;
        private readonly ClubType _includedClubFlag;
        
        public MoveRandomCardsBelongClubsFromInfirmaryToDeck(Card card, JsonObject json) : base(card, json)
        {
            foreach (var field in json.fields)
            {
                switch (field.key)
                {
                    case "cards_amount":
                        _cardsAmount = field.value.intValue;
                        break;
                    case "club_includes":
                        
                        ClubType includeFlag = 0;
                        
                        foreach (var element in field.value.arr)
                        {
                            includeFlag |= Enum.Parse<ClubType>(element.strValue, true);
                        }

                        _includedClubFlag = includeFlag;
                        break;
                }
            }
        }

        public override void CheckApplyEffect(CardEffectArgs args, IMatchContextEvent matchContextEvent)
        {
            var (trigger, ownSide, otherSide, gameState) = args;

            if (!ApplyTriggerFlag.HasFlag(trigger))
            {
                return;
            }
            
            // TODO: use PCG32
            Random random = new Random();
            var cardsBelongClubsInInfirmary = ownSide.Infirmary.GetAllCards()
                .Where(card => _includedClubFlag.HasFlag(card.ClubType))
                .OrderBy(_ => random.Next())
                .Take(_cardsAmount)
                .ToArray();

            if (cardsBelongClubsInInfirmary.Length == 0)
            {
                CallCard.TryGetCardLocation(ownSide, out var currentLocation);

                FailToActivateCardEffectEvent failToActivateEvent = new FailToActivateCardEffectEvent(
                    CallCard.Id,
                    currentLocation,
                    FailToActivateEffectReason.NoMeetCondition
                );
                
                failToActivateEvent.RegisterEvent(matchContextEvent);
                
                // var failEffectEvent = new FailToApplyCardEffectEvent(FailToApplyCardEffectEvent.FailReason.NoInfirmaryRemains);
                // failEffectEvent.RegisterEvent(matchContextEvent);
                return;
            }

            CardEffectArgs onLeaveInfirmaryArgs = new CardEffectArgs(
                EffectTriggerEvent.OnLeaveInfirmary,
                ownSide,
                otherSide,
                gameState
            );

            foreach (var cardToMove in cardsBelongClubsInInfirmary)
            {
                string slotKey = cardToMove.CardData.nameKey;
                
                if (!ownSide.Infirmary.TryGetValue(slotKey, out var cardPile))
                {
                    continue;
                }

                int indexOfSlot = cardPile.IndexOf(cardToMove);
                CardLocation prevAppliedCardLocation = new InfirmaryLocation(ownSide.Player, slotKey, indexOfSlot);

                cardPile.Remove(cardToMove);
                if (cardPile.Count == 0)
                {
                    ownSide.Infirmary.Remove(slotKey);
                }
                
                ownSide.Deck.AddToBottom(cardToMove);

                CallCard.TryGetCardLocation(ownSide, out CardLocation activatedCardLocation);
                CardEffectAppliedInfo appliedInfo = new CardEffectAppliedInfo(prevAppliedCardLocation, activatedCardLocation);

                CardLocation curLocation = new DeckLocation(ownSide.Player, 0);
                CardMovementInfo movementInfo = new CardMovementInfo(prevAppliedCardLocation, curLocation);
                
                MoveCardByEffectEvent moveCardEvent = new MoveCardByEffectEvent(appliedInfo, movementInfo);
                moveCardEvent.RegisterEvent(matchContextEvent);
                    
                // string moveCardToBottomOfDeckMessage = $"{ownSide.Player.Name}가 카드를 덱 맨 아래로 보냄\n{cardToMove}";
                // var moveCardEffectEvent = new CommonMatchMessageEvent(moveCardToBottomOfDeckMessage);
                // moveCardEffectEvent.RegisterEvent(matchContextEvent);
                
                cardToMove.CardEffect.CheckApplyEffect(onLeaveInfirmaryArgs, matchContextEvent);

                if (matchContextEvent.MatchFinished)
                {
                    return;
                }
            }
        }
    }
}
