using System;
using System.Collections.Generic;
using ProjectABC.Data;


namespace ProjectABC.Core
{
    /// <summary>
    /// 상대의 매치 승리 횟수 n 만큼 파워 증가
    /// </summary>
    public sealed class PowerUpSelfAsEachOpponentWonCount : CardEffect
    {
        private readonly EffectTriggerEvent _cancelTriggerFlag;
        private readonly int _powerUpRatio;
        
        public PowerUpSelfAsEachOpponentWonCount(Card card, JsonObject json) : base(card, json)
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
                ExclusiveCardBuff cardBuff = new ExclusiveCardBuff(CallCard, _powerUpRatio);
                var handler = new CardBuffHandleEntry(CallCard, cardBuff);
                
                ownSide.CardBuffHandlers.Add(handler);
            }
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
                return new[] { CallCard };
            }

            public override bool IsBuffActive(Card target, CardBuffArgs args)
            {
                return args.OwnSide.IsEffectiveStandOnField(target)
                       && args.OwnSide.IsAttacking;
            }

            public override int CalculateAdditivePower(Card target, CardBuffArgs args)
            {
                var (ownSide, otherSide, gameState) = args;

                int lastRound = gameState.Round - 1;
                int wonCount = 0;

                if (lastRound > 0)
                {
                    IPlayer opponent = otherSide.Player;
                    for (int i = 1; i <= lastRound; i++)
                    {
                        var previousMatchResult = gameState.MatchResults.GetMatchResult(i, opponent);
                        wonCount += previousMatchResult.Winner == opponent
                            ? 1
                            : 0;
                    }
                }

                return wonCount * _powerUpRatio;
            }
        }
    }
}