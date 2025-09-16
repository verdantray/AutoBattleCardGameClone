using System;
using System.Collections.Generic;
using ProjectABC.Data;


namespace ProjectABC.Core
{
    public sealed class PowerUpSelfByOpponentDeckStock : CardEffect
    {
        private readonly EffectTriggerEvent _cancelTriggerFlag;
        private readonly int _opponentDeckStock;
        private readonly int _powerUpBonus;
        
        public PowerUpSelfByOpponentDeckStock(Card card, JsonObject json) : base(card, json)
        {
            foreach (var field in json.fields)
            {
                switch (field.key)
                {
                    case GameConst.CardEffect.EFFECT_CANCEL_TRIGGERS_KEY:
                        
                        EffectTriggerEvent flag = 0;
                        
                        foreach (var element in field.value.arr)
                        {
                            flag |= Enum.Parse<EffectTriggerEvent>(element.strValue, true);
                        }

                        _cancelTriggerFlag = flag;
                        break;
                    case "opponent_deck_stock":
                        _opponentDeckStock = field.value.intValue;
                        break;
                    case "power_up_bonus":
                        _powerUpBonus = field.value.intValue;
                        break;
                }
            }
        }

        public override void CheckApplyEffect(CardEffectArgs args, IMatchContextEvent matchContextEvent)
        {
            var (trigger, ownSide, otherSide, gameState) = args;
            
            bool isApplyTrigger = ApplyTriggerFlag.HasFlag(trigger);
            bool isCancelTrigger = _cancelTriggerFlag.HasFlag(trigger);

            bool isBuffActive = ownSide.CardBuffHandlers.Exists(entry => entry.CallCard == CallCard);

            // case : already buff active, but effect canceled
            if (isBuffActive && isCancelTrigger)
            {
                var handlers = ownSide.CardBuffHandlers.FindAll(entry => entry.CallCard == CallCard);
                
                foreach (var handler in handlers)
                {
                    handler.Release();
                    ownSide.CardBuffHandlers.Remove(handler);
                }

                var inactiveBuffEvent = new CommonMatchMessageEvent($"{CallCard.Title} {CallCard.Name}의 효과가 취소됩니다. / {CallCard.CardEffect.Description}");
                inactiveBuffEvent.RegisterEvent(matchContextEvent);

                return;
            }

            // case : buff not active yet, and effect triggered
            if (!isBuffActive && isApplyTrigger)
            {
                ExclusiveCardBuff cardBuff = new ExclusiveCardBuff(CallCard, _opponentDeckStock, _powerUpBonus);
                var handler = new CardBuffHandleEntry(CallCard, cardBuff);
                
                ownSide.CardBuffHandlers.Add(handler);
                
                var activeBuffEvent = new CommonMatchMessageEvent($"{CallCard.Title} {CallCard.Name}의 효과가 발동됩니다. / {CallCard.CardEffect.Description}");
                activeBuffEvent.RegisterEvent(matchContextEvent);
            }
        }

        protected override string GetDescription()
        {
            return LocalizationHelper.Instance.Localize(DescriptionKey, _opponentDeckStock, _powerUpBonus);
        }

        private sealed class ExclusiveCardBuff : CardBuff
        {
            public override BuffType Type => BuffType.Positive;
            
            private readonly int _opponentDeckStock;
            private readonly int _powerUpBonus;
            
            public ExclusiveCardBuff(Card callCard, int opponentDeckStock, int powerUpBonus) : base(callCard)
            {
                _opponentDeckStock = opponentDeckStock;
                _powerUpBonus = powerUpBonus;
            }
            
            public override IEnumerable<Card> GetBuffTargets(CardBuffArgs args)
            {
                return args.OwnSide.Field.Contains(CallCard)
                    ? new[] { CallCard }
                    : Array.Empty<Card>();
            }

            public override bool IsBuffActive(Card target, CardBuffArgs args)
            {
                return args.OwnSide.IsEffectiveStandOnField(target)
                       && args.OtherSide.Deck.Count >= _opponentDeckStock;
            }

            public override int CalculateAdditivePower(Card target, CardBuffArgs args)
            {
                return _powerUpBonus;
            }
        }
    }
}