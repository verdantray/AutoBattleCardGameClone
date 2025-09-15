using System;
using System.Linq;
using ProjectABC.Data;

namespace ProjectABC.Core
{
    public sealed class MoveRandomCardsToField : CardEffect
    {
        private readonly int _cardsAmount;
        
        public MoveRandomCardsToField(Card card, JsonObject json) : base(card, json)
        {
            foreach (var field in json.fields)
            {
                switch (field.key)
                {
                    case "cards_amount":
                        _cardsAmount = field.value.intValue;
                        break;
                }
            }
        }

        public override void CheckApplyEffect(CardEffectArgs args, MatchContextEvent matchContextEvent)
        {
            var (trigger, ownSide, otherSide, gameState) = args;

            if (!ApplyTriggerFlag.HasFlag(trigger))
            {
                return;
            }
            
            // TODO: PCG32
            Random random = new Random();
            var cardsInInfirmary = ownSide.Infirmary.GetAllCards()
                .OrderBy(_ => random.Next())
                .Take(_cardsAmount)
                .ToArray();

            if (cardsInInfirmary.Length == 0)
            {
                var failEffectEvent = new FailToApplyCardEffectEvent(
                    FailToApplyCardEffectEvent.FailReason.NoInfirmaryRemains,
                    new MatchSnapshot(ownSide, otherSide)
                );
                
                failEffectEvent.RegisterEvent(matchContextEvent);
                return;
            }
            
            CardEffectArgs onLeaveInfirmaryArgs = new CardEffectArgs(
                EffectTriggerEvent.OnLeaveInfirmary,
                ownSide,
                otherSide,
                gameState
            );

            CardEffectArgs onEnterFieldArgs = new CardEffectArgs(
                ownSide.IsAttacking
                    ? EffectTriggerEvent.OnEnterFieldAsAttacker
                    : EffectTriggerEvent.OnEnterFieldAsDefender,
                ownSide,
                otherSide,
                gameState
            );

            for (int i = 0; i < _cardsAmount; i++)
            {
                if (cardsInInfirmary.Length <= i)
                {
                    break;
                }
                
                Card cardToMove = cardsInInfirmary[i];

                for (int j = ownSide.Infirmary.Count - 1; j >= 0; j--)
                {
                    if (!ownSide.Infirmary[j].Contains(cardToMove))
                    {
                        continue;
                    }

                    ownSide.Infirmary[j].Remove(cardToMove);
                    if (ownSide.Infirmary.Count == 0)
                    {
                        ownSide.Infirmary.RemoveByIndex(j);
                    }
                }
                
                cardToMove.CardEffect.CheckApplyEffect(onLeaveInfirmaryArgs, matchContextEvent);
                
                // set trigger to last one of field if ownSide is defending
                if (!ownSide.IsAttacking)
                {
                    CardEffectArgs remainFieldEffectArgs = new CardEffectArgs(
                        EffectTriggerEvent.OnRemainField,
                        ownSide,
                        otherSide,
                        gameState
                    );
                    
                    ownSide.Field[^1].CardEffect.CheckApplyEffect(remainFieldEffectArgs, matchContextEvent);
                    if (matchContextEvent.MatchFinished)
                    {
                        return;
                    }
                }
                
                ownSide.Field.Add(cardToMove);
                cardToMove.CardEffect.CheckApplyEffect(onEnterFieldArgs, matchContextEvent);
                if (matchContextEvent.MatchFinished)
                {
                    return;
                }
            }
        }

        protected override string GetDescription()
        {
            return LocalizationHelper.Instance.Localize(DescriptionKey, _cardsAmount);
        }
    }
}