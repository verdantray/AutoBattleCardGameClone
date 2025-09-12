using System;
using System.Collections.Generic;
using ProjectABC.Data;


namespace ProjectABC.Core
{
    /// <summary>
    /// 상대 필드의 마지막 카드의 학년 수치 n만큼 파워 증가
    /// </summary>
    public sealed class PowerUpSelfAsEachGradeOfOpponentFieldLast : CardEffect
    {
        private readonly EffectTriggerEvent _cancelTriggerFlag;
        private readonly int _powerUpRatio;
        
        public PowerUpSelfAsEachGradeOfOpponentFieldLast(Card card, JsonObject json) : base(card, json)
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
                    case "power_up_ratio":
                        _powerUpRatio = field.value.intValue;
                        break;
                }
            }
        }

        public override void CheckApplyEffect(CardEffectArgs args, MatchContextEvent matchContextEvent)
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

                var inactiveBuffEvent = new InactiveBuffEvent(CallCard, new MatchSnapshot(ownSide, otherSide));
                inactiveBuffEvent.RegisterEvent(matchContextEvent);
                
                return;
            }
            
            // case : buff not active yet, and effect triggered
            if (!isBuffActive && isApplyTrigger)
            {
                ExclusiveCardBuff cardBuff = new ExclusiveCardBuff(CallCard, _powerUpRatio);
                var handler = new CardBuffHandleEntry(CallCard, cardBuff);
                
                ownSide.CardBuffHandlers.Add(handler);
                
                var activeBuffEvent = new ActiveCardBuffEvent(CallCard, new MatchSnapshot(ownSide, otherSide));
                activeBuffEvent.RegisterEvent(matchContextEvent);
            }
        }

        protected override string GetDescription()
        {
            // TODO: localization
            return DescriptionKey;
        }
        
        private sealed class ExclusiveCardBuff : CardBuff
        {
            public override BuffType Type => BuffType.Positive;

            private readonly int _powerUpRatio;
            
            public ExclusiveCardBuff(Card callCard, int powerUpRatio) : base(callCard)
            {
                _powerUpRatio = powerUpRatio;
            }
            
            public override IEnumerable<Card> GetBuffTargets(CardBuffArgs args)
            {
                return args.OwnSide.Field.Contains(CallCard)
                    ? new[] { CallCard }
                    : Array.Empty<Card>();
            }

            public override bool IsBuffActive(Card target, CardBuffArgs args)
            {
                return args.OwnSide.IsEffectiveStandOnField(target);
            }

            public override int CalculateAdditivePower(Card target, CardBuffArgs args)
            {
                GradeType gradeOfOpponentFieldLast = args.OtherSide.Field[^1].GradeType;
                int powerFromGrade = gradeOfOpponentFieldLast switch
                {
                    GradeType.First => 1,
                    GradeType.Second => 2,
                    GradeType.Third => 3,
                    _ => throw new ArgumentOutOfRangeException()
                };

                return powerFromGrade * _powerUpRatio;
            }
        }
    }
}