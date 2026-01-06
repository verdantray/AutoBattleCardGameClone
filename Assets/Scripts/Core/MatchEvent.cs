using System.Collections.Generic;
using System.Linq;

namespace ProjectABC.Core
{
    public interface IMatchEvent
    {
        public void RegisterEvent(IMatchContextEvent matchContextEvent);
    }

    public abstract class MatchEvent : IMatchEvent
    {
        public virtual void RegisterEvent(IMatchContextEvent matchContextEvent)
        {
            if (matchContextEvent.MatchFinished)
            {
                return;
            }

            matchContextEvent.MatchEvents.Add(this);
        }
    }

    public class MatchStartEvent : MatchEvent
    {
        public readonly IPlayer Attacker;
        public readonly IPlayer Defender;
        public readonly PositionDecideReason Reason;
        public readonly MatchSnapshot MatchMatchSnapshot;

        public MatchStartEvent(IPlayer attacker, IPlayer defender, PositionDecideReason reason, MatchSnapshot matchSnapshot)
        {
            Attacker = attacker;
            Defender = defender;
            Reason = reason;
            MatchMatchSnapshot = matchSnapshot;
        }
    }

    public class MatchFinishEvent : MatchEvent
    {
        public readonly IPlayer Winner;
        public readonly IPlayer Loser;
        public readonly MatchEndReason Reason;

        public MatchFinishEvent(IPlayer winner, IPlayer loser, MatchEndReason reason)
        {
            Winner = winner;
            Loser = loser;
            Reason = reason;
        }

        public override void RegisterEvent(IMatchContextEvent matchContextEvent)
        {
            if (matchContextEvent.MatchFinished)
            {
                return;
            }

            matchContextEvent.SetResult(Winner, Loser, Reason);
            matchContextEvent.MatchEvents.Add(this);
        }
    }

    public record CardMovementInfo
    {
        public readonly CardLocation PreviousLocation;
        public readonly CardLocation CurrentLocation;

        public CardMovementInfo(CardLocation previousLocation, CardLocation currentLocation)
        {
            PreviousLocation = previousLocation;
            CurrentLocation = currentLocation;
        }
    }

    public class DrawCardToFieldEvent : MatchEvent
    {
        public readonly IPlayer Owner;
        public readonly CardMovementInfo MovementInfo;

        public DrawCardToFieldEvent(IPlayer owner, CardMovementInfo movementInfo)
        {
            Owner = owner;
            MovementInfo = movementInfo;
        }
    }

    public class SuccessAttackEvent : MatchEvent
    {
        public readonly IPlayer Attacker;

        public SuccessAttackEvent(IPlayer attacker)
        {
            Attacker = attacker;
        }
    }

    public class SendToInfirmaryEvent : MatchEvent
    {
        public readonly IPlayer Owner;
        public readonly CardMovementInfo MovementInfo;

        public SendToInfirmaryEvent(IPlayer owner, CardMovementInfo movementInfo)
        {
            Owner = owner;
            MovementInfo = movementInfo;
        }
    }

    public class CardEffectAppliedInfo
    {
        public readonly CardLocation AppliedCardLocation;
        public readonly CardLocation ActivatedCardLocation;

        public CardEffectAppliedInfo(CardLocation selfActivatedCardLocation)
        {
            AppliedCardLocation = selfActivatedCardLocation;
            ActivatedCardLocation = selfActivatedCardLocation;
        }

        public CardEffectAppliedInfo(CardLocation appliedCardLocation, CardLocation activatedCardLocation)
        {
            AppliedCardLocation = appliedCardLocation;
            ActivatedCardLocation = activatedCardLocation;
        }
    }
    
    public class DrawCardFromPileEvent : MatchEvent
    {
        public readonly CardLocation ActivatedCardLocation;
        public readonly CardReference DrawnCard;
        public readonly CardMovementInfo MovementInfo;
        
        public DrawCardFromPileEvent(CardLocation activatedCardLocation, CardReference drawnCard, CardMovementInfo movementInfo)
        {
            ActivatedCardLocation = activatedCardLocation;
            DrawnCard = drawnCard;
            MovementInfo = movementInfo;
        }
    }

    public class MoveCardByEffectEvent : MatchEvent
    {
        public readonly CardEffectAppliedInfo AppliedInfo;
        public readonly CardMovementInfo MovementInfo;

        public MoveCardByEffectEvent(CardEffectAppliedInfo appliedInfo, CardMovementInfo movementInfo)
        {
            AppliedInfo = appliedInfo;
            MovementInfo = movementInfo;
        }
    }

    public class ShuffleDeckEvent : MatchEvent
    {
        public readonly CardEffectAppliedInfo AppliedInfo;
        public readonly CardMovementInfo MovementInfo;
        
        public ShuffleDeckEvent(CardEffectAppliedInfo appliedInfo, CardMovementInfo movementInfo)
        {
            AppliedInfo = appliedInfo;
            MovementInfo = movementInfo;
        }
    }

    public class GainWinPointsByCardEffectEvent : MatchEvent
    {
        public readonly IPlayer Player;
        public readonly string ActivatedCardId;
        public readonly CardLocation ActivatedCardLocation;
        public readonly int GainPoints;
        public readonly int TotalPoints;

        public GainWinPointsByCardEffectEvent(IPlayer player, string activatedCardId, CardLocation currentLocation, int gainPoints, int totalPoints)
        {
            Player = player;
            ActivatedCardId = activatedCardId;
            ActivatedCardLocation = currentLocation;
            GainPoints = gainPoints;
            TotalPoints = totalPoints;
        }
    }

    public enum FailToActivateEffectReason
    {
        NoMeetCondition,
        NoDeckRemains,
        NoCardPileRemains,
        NoOpponentCardPileRemains,
        NoInfirmaryRemains,
        NoOpponentInfirmaryRemains,
    }

    public class FailToActivateCardEffectEvent : MatchEvent
    {
        public readonly string FailedCardId;
        public readonly CardLocation FailedCardLocation;
        public readonly FailToActivateEffectReason Reason;

        public FailToActivateCardEffectEvent(string failedCardId, CardLocation failedCardLocation, FailToActivateEffectReason reason)
        {
            FailedCardId = failedCardId;
            FailedCardLocation = failedCardLocation;
            Reason = reason;
        }
    }

    public class SwitchPositionEvent : MatchEvent
    {
        public readonly IPlayer Defender;
        public readonly IPlayer Attacker;
        public readonly MatchSnapshot MatchSnapshot;

        public SwitchPositionEvent(IPlayer defender, IPlayer attacker, MatchSnapshot matchSnapshot)
        {
            Defender = defender;
            Attacker = attacker;
            MatchSnapshot = matchSnapshot;
        }
    }

    public class BuffTarget
    {
        public readonly CardReference TargetReference;
        public readonly CardLocation TargetLocation;

        public BuffTarget(CardReference targetReference, CardLocation targetLocation)
        {
            TargetReference = targetReference;
            TargetLocation = targetLocation;
        }

        public void Deconstruct(out CardReference targetReference, out CardLocation targetLocation)
        {
            targetReference = TargetReference;
            targetLocation = TargetLocation;
        }
    }

    public class ActiveBuffEvent : MatchEvent
    {
        public readonly string ActiveCardId;
        public readonly CardLocation ActivatedCardLocation;
        public readonly IReadOnlyList<BuffTarget> BuffApplyTargets;
        
        public ActiveBuffEvent(Card activeCard, IEnumerable<Card> appliedCards, CardBuffArgs args)
        {
            ActiveCardId = activeCard.Id;
            ActivatedCardLocation = activeCard.GetCardLocation(args.OwnSide, args.OtherSide);
            BuffApplyTargets = appliedCards.Select(GetBuffTarget).ToList();
            return;

            BuffTarget GetBuffTarget(Card appliedCard)
            {
                CardReference appliedReference = new CardReference(appliedCard, args);
                appliedCard.TryGetCardLocation(args.OwnSide, out var appliedLocation);
                
                return new BuffTarget(appliedReference, appliedLocation);
            }
        }
    }

    public class InactiveBuffEvent : MatchEvent
    {
        public readonly string InactiveCardId;
        public readonly CardLocation InactivatedCardLocation;
        public readonly IReadOnlyList<BuffTarget> BuffCancelTargets;
        
        public InactiveBuffEvent(Card inactiveCard, IEnumerable<Card> canceledCards, CardBuffArgs args)
        {
            InactiveCardId = inactiveCard.Id;
            InactivatedCardLocation = inactiveCard.GetCardLocation(args.OwnSide, args.OtherSide);
            BuffCancelTargets = canceledCards.Select(GetBuffTarget).ToList();
            return;

            BuffTarget GetBuffTarget(Card appliedCard)
            {
                CardReference appliedReference = new CardReference(appliedCard, args);
                appliedCard.TryGetCardLocation(args.OwnSide, out var appliedLocation);
                
                return new BuffTarget(appliedReference, appliedLocation);
            }
        }
    }
}
