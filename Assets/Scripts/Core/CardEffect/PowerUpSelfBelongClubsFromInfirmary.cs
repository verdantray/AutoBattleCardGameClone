using System;
using System.Collections.Generic;
using System.Linq;
using ProjectABC.Data;


namespace ProjectABC.Core
{
    /// <summary>
    /// 양호실에 특정 동아리 소속이 있는 경우 자신의 파워 N 증가
    /// </summary>
    public sealed class PowerUpSelfBelongClubsFromInfirmary : CardEffect
    {
        private readonly EffectTriggerEvent _cancelTriggerFlag;
        private readonly ClubType _includedClubFlag;
        private readonly int _powerUpBonus;
        
        public PowerUpSelfBelongClubsFromInfirmary(Card card, JsonObject json) : base(card, json)
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
                    case "club_includes":

                        ClubType includeFlag = 0;

                        foreach (var element in field.value.arr)
                        {
                            includeFlag |= Enum.Parse<ClubType>(element.strValue, true);
                        }

                        _includedClubFlag = includeFlag;
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
                CardBuffArgs cardBuffArgs = new CardBuffArgs(ownSide, otherSide, gameState);
                
                foreach (var handler in handlers)
                {
                    var buffCancelResult = handler.Release();
                    if (buffCancelResult.TryGetInactiveBuffEvent(cardBuffArgs, out var inactiveBuffEvent))
                    {
                        inactiveBuffEvent.RegisterEvent(matchContextEvent);
                    }
                    
                    ownSide.CardBuffHandlers.Remove(handler);
                }

                return;
            }

            // case : buff not active yet, and effect triggered
            if (!isBuffActive && isApplyTrigger)
            {
                ExclusiveCardBuff cardBuff = new ExclusiveCardBuff(CallCard, _includedClubFlag, _powerUpBonus);
                var handler = new CardBuffHandleEntry(CallCard, cardBuff);
                
                ownSide.CardBuffHandlers.Add(handler);
            }
        }

        private sealed class ExclusiveCardBuff : CardBuff
        {
            public override BuffType Type => BuffType.Positive;
            
            private readonly ClubType _includedClubFlag;
            private readonly int _powerUpBonus;
            
            public ExclusiveCardBuff(Card callCard, ClubType includedClubFlag, int powerUpBonus) : base(callCard)
            {
                _includedClubFlag = includedClubFlag;
                _powerUpBonus = powerUpBonus;
            }
            
            public override IEnumerable<Card> GetBuffTargets(CardBuffArgs args)
            {
                return new[] { CallCard };
            }

            public override bool IsBuffActive(Card target, CardBuffArgs args)
            {
                bool isTargetEffectiveStand = args.OwnSide.IsEffectiveStandOnField(target);
                bool isClubExistsInInfirmary = args.OwnSide.Infirmary.GetAllCards()
                    .Any(card => _includedClubFlag.HasFlag(card.ClubType));
                
                return isTargetEffectiveStand && isClubExistsInInfirmary;
            }

            public override int CalculateAdditivePower(Card target, CardBuffArgs args)
            {
                return _powerUpBonus;
            }
        }
    }
}
