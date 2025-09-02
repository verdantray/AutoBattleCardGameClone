using System;
using System.Collections.Generic;
using System.Linq;
using ProjectABC.Data;

namespace ProjectABC.Core
{
    public class PowerUpAsEachClubsFromInfirmary : CardEffect
    {
        private readonly EffectTriggerEvent _cancelTriggerFlag;
        private readonly ClubType _excludedClubFlag;
        private readonly int _powerUpRatio;
        
        public PowerUpAsEachClubsFromInfirmary(Card card, JsonObject json) : base(card, json)
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
                    case "club_excludes":
                        break;
                    case "power_up_ratio":
                        break;
                }
            }
        }

        public override bool TryApplyEffect(EffectTriggerEvent trigger, MatchSide mySide, MatchSide otherSide, out IMatchEvent matchEvent)
        {
            bool isApplyTrigger = ApplyTriggerFlag.HasFlag(trigger);
            bool isCancelTrigger = _cancelTriggerFlag.HasFlag(trigger);

            bool isBuffActive = mySide.CardBuffHandlers.Exists(entry => entry.CallCard == CallCard);

            // case : already buff active, but effect canceled
            if (isBuffActive && isCancelTrigger)
            {
                var handlers = mySide.CardBuffHandlers.FindAll(entry => entry.CallCard == CallCard);
                
                foreach (var handler in handlers)
                {
                    handler.Release();
                    mySide.CardBuffHandlers.Remove(handler);
                }

                matchEvent = new InactiveBuffEvent(CallCard, new MatchSnapshot(mySide, otherSide));
                return true;
            }

            // case : buff not active yet, and effect triggered
            if (!isBuffActive && isApplyTrigger)
            {
                ExclusiveCardBuff cardBuff = new ExclusiveCardBuff(_excludedClubFlag, _powerUpRatio);
                var handler = new CardBuffHandleEntry(CallCard, cardBuff, BuffTargetSelector);
                
                mySide.CardBuffHandlers.Add(handler);
                
                matchEvent = new ActiveCardBuffEvent(CallCard, new MatchSnapshot(mySide, otherSide));
                return true;
            }

            matchEvent = null;
            return false;
        }

        private IEnumerable<Card> BuffTargetSelector(MatchSide matchSide)
        {
            return matchSide.Field.Contains(CallCard)
                ? new[] { CallCard }
                : Array.Empty<Card>();
        }

        protected override string GetDescription()
        {
            // TODO: localization
            return "";
        }
        
        private class ExclusiveCardBuff : CardBuff
        {
            private readonly ClubType _excludedClubFlag;
            private readonly int _powerUpRatio;
        
            public ExclusiveCardBuff(ClubType excludedClubFlag, int powerUpRatio)
            {
                _excludedClubFlag = excludedClubFlag;
                _powerUpRatio = powerUpRatio;
            }
        
            public override bool IsBuffActive(Card target, MatchSide matchSide)
            {
                int clubCountInInfirmary = matchSide.Infirmary.GetAllCards()
                    .Select(card => card.ClubType)
                    .Distinct()
                    .Count(club => !_excludedClubFlag.HasFlag(club));

                return clubCountInInfirmary > 0;
            }

            public override int CalculateAdditivePower(Card target, MatchSide matchSide)
            {
                int clubCountInInfirmary = matchSide.Infirmary.GetAllCards()
                    .Select(card => card.ClubType)
                    .Distinct()
                    .Count(club => !_excludedClubFlag.HasFlag(club));
            
                return clubCountInInfirmary * _powerUpRatio;
            }
        }
    }
}
