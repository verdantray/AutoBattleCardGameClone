using System;
using System.Collections.Generic;
using ProjectABC.Data;


namespace ProjectABC.Core
{
    public enum LastRoundResult
    {
        Win,
        Lose,
    }
    
    public class PowerUpSelfByLastMatch : CardEffect
    {
        
        private readonly EffectTriggerEvent _cancelTriggerFlag;
        private readonly LastRoundResult _enableLastResult;
        private readonly int _powerUpBonus;
        
        public PowerUpSelfByLastMatch(Card card, JsonObject json) : base(card, json)
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
                    case "enable_last_result":
                        _enableLastResult = Enum.Parse<LastRoundResult>(field.value.strValue, true);
                        break;
                    case "power_up_bonus":
                        _powerUpBonus = field.value.intValue;
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
                ExclusiveCardBuff cardBuff = new ExclusiveCardBuff(CallCard, _enableLastResult, _powerUpBonus);
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
        
        private class ExclusiveCardBuff : CardBuff
        {
            public override BuffType Type => BuffType.Positive;
            
            private readonly LastRoundResult _enableLastResult;
            private readonly int _powerUpBonus;
            
            public ExclusiveCardBuff(Card callCard, LastRoundResult enableLastResult, int powerUpBonus) : base(callCard)
            {
                _enableLastResult = enableLastResult;
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
                IPlayer ownPlayer = args.OwnSide.Player;
                int lastRound = args.GameState.Round - 1;

                if (lastRound <= 0)
                {
                    return false;
                }

                var lastMatchResult = args.GameState.MatchResults.GetMatchResult(lastRound, ownPlayer);

                bool isTargetEffectiveStand = args.OwnSide.IsEffectiveStandOnField(target);
                bool isEnableResultLastMatch = _enableLastResult == LastRoundResult.Win
                    ? lastMatchResult.Winner == ownPlayer
                    : lastMatchResult.Loser == ownPlayer;
                
                return isEnableResultLastMatch && isTargetEffectiveStand;
            }

            public override int CalculateAdditivePower(Card target, CardBuffArgs args)
            {
                return _powerUpBonus;
            }
        }
    }
}