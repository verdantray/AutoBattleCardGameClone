using System;
using System.Collections.Generic;
using System.Linq;
using ProjectABC.Data;

namespace ProjectABC.Core
{
    /// <summary>
    /// 양호실로 보내질 때 : 특정 동아리 소속의 카드가 공격 / 수비 중일 때 파워 + N 만큼 증가
    /// </summary>
    public sealed class GivePowerUpToBelongClubsFromInfirmary : CardEffect
    {
        private readonly EffectTriggerEvent _cancelTriggerFlag;
        private readonly ClubType _includedClubFlag;
        private readonly MatchPosition _enablePositionFlag;
        private readonly int _powerUpBonus;
        
        public GivePowerUpToBelongClubsFromInfirmary(Card card, JsonObject json) : base(card, json)
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
                    case "enable_match_positions":
                        MatchPosition positionFlag = 0;

                        foreach (var element in field.value.arr)
                        {
                            positionFlag |= Enum.Parse<MatchPosition>(element.strValue, true);
                        }
                        _enablePositionFlag = positionFlag;
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
                
                return;
            }
            
            // case : buff not active yet, and effect triggered
            if (!isBuffActive && isApplyTrigger)
            {
                ExclusiveCardBuff cardBuff = new ExclusiveCardBuff(CallCard, _includedClubFlag, _enablePositionFlag, _powerUpBonus);
                var handler = new CardBuffHandleEntry(CallCard, cardBuff);
                
                ownSide.CardBuffHandlers.Add(handler);
            }
        }

        private sealed class ExclusiveCardBuff : CardBuff
        {
            public override BuffType Type => BuffType.Positive;
            
            private readonly ClubType _includedClubFlag;
            private readonly MatchPosition _enablePositionFlag;
            private readonly int _powerUpBonus;

            public ExclusiveCardBuff(Card callCard, ClubType includedClubFlag, MatchPosition enablePositionFlag, int powerUpBonus) : base(callCard)
            {
                _includedClubFlag = includedClubFlag;
                _enablePositionFlag = enablePositionFlag;
                _powerUpBonus = powerUpBonus;
            }

            public override IEnumerable<Card> GetBuffTargets(CardBuffArgs args)
            {
                string nameKey = CallCard.CardData.nameKey;
                bool isCallCardRemainInfirmary = args.OwnSide.Infirmary.TryGetValue(nameKey, out var cardPile)
                                                 && cardPile.Contains(CallCard);
                return isCallCardRemainInfirmary
                    ? args.OwnSide.Field.Where(card => _includedClubFlag.HasFlag(card.ClubType))
                    : Array.Empty<Card>();
            }

            public override bool IsBuffActive(Card target, CardBuffArgs args)
            {
                HashSet<Card> infirmaryCardSet = new HashSet<Card>(args.OwnSide.Infirmary.GetAllCards());

                bool isCallerInInfirmary = infirmaryCardSet.Contains(CallCard);
                bool isTargetEffectiveStand = args.OwnSide.IsEffectiveStandOnField(target);
                bool isTargetBelongingClub = _includedClubFlag.HasFlag(target.ClubType);
                bool isEnableMatchState = _enablePositionFlag.HasFlag(args.OwnSide.Position);
                
                return isCallerInInfirmary && isTargetEffectiveStand && isTargetBelongingClub && isEnableMatchState;
            }

            public override int CalculateAdditivePower(Card target, CardBuffArgs args)
            {
                return _powerUpBonus;
            }
        }
    }
}
