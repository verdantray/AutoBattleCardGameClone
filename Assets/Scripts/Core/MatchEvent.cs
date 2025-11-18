

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
        public readonly CardReference AppliedCard;
        public readonly CardReference ActivatedCard;

        public CardEffectAppliedInfo(CardReference selfActivatedCard)
        {
            AppliedCard = selfActivatedCard;
            ActivatedCard =  selfActivatedCard;
        }

        public CardEffectAppliedInfo(CardReference appliedCard, CardReference activatedCard)
        {
            AppliedCard = appliedCard;
            ActivatedCard = activatedCard;
        }
    }

    public class SendToInfirmaryFromDeckEvent : MatchEvent
    {
        public readonly CardEffectAppliedInfo AppliedInfo;
        public readonly CardMovementInfo MovementInfo;
        
        public SendToInfirmaryFromDeckEvent(CardEffectAppliedInfo appliedInfo, CardMovementInfo movementInfo)
        {
            AppliedInfo = appliedInfo;
            MovementInfo = movementInfo;
        }
    }

    public class SendToDeckFromInfirmaryEvent : MatchEvent
    {
        public readonly CardEffectAppliedInfo AppliedInfo;
        public readonly CardMovementInfo MovementInfo;
        
        public SendToDeckFromInfirmaryEvent(CardEffectAppliedInfo appliedInfo, CardMovementInfo movementInfo)
        {
            AppliedInfo = appliedInfo;
            MovementInfo = movementInfo;
        }
    }

    public class DrawCardByEffectEvent : MatchEvent
    {
        public readonly CardEffectAppliedInfo AppliedInfo;
        public readonly CardMovementInfo MovementInfo;
        
        public DrawCardByEffectEvent(CardEffectAppliedInfo appliedInfo, CardMovementInfo movementInfo)
        {
            AppliedInfo = appliedInfo;
            MovementInfo = movementInfo;
        }
    }

    public class SendToDeckInsteadOfInfirmaryEvent : MatchEvent
    {
        public readonly CardEffectAppliedInfo AppliedInfo;
        public readonly CardMovementInfo MovementInfo;
        
        public SendToDeckInsteadOfInfirmaryEvent(CardEffectAppliedInfo appliedInfo, CardMovementInfo movementInfo)
        {
            AppliedInfo = appliedInfo;
            MovementInfo = movementInfo;
        }
    }

    public class ShuffleDeckEvent : MatchEvent
    {
        public readonly CardEffectAppliedInfo AppliedInfo;
        
        public ShuffleDeckEvent(CardEffectAppliedInfo appliedInfo)
        {
            AppliedInfo = appliedInfo;
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
        public readonly CardReference FailedCard;
        public readonly FailToActivateEffectReason Reason;

        public FailToActivateCardEffectEvent(CardReference failedCard, FailToActivateEffectReason reason)
        {
            FailedCard = failedCard;
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

    public class ActiveBuffEvent : MatchEvent
    {
        public readonly CardReference Card;
        
        public ActiveBuffEvent(CardReference card)
        {
            Card = card;
        }
    }

    public class InactiveBuffEvent : MatchEvent
    {
        public readonly CardReference Card;
        
        public InactiveBuffEvent(CardReference card)
        {
            Card = card;
        }
    }
}
