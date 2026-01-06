using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectABC.Core
{
    public enum PositionDecideReason
    {
        DecidedByWinPoints,
        DecidedByRandom,
    }
    
    public interface IMatchContextEvent : IContextEvent
    {
        public MatchResult Result { get; }
        public bool MatchFinished { get; }
        public List<IMatchEvent> MatchEvents { get; }

        public void SetResult(IPlayer winPlayer, IPlayer losePlayer, MatchEndReason reason);
        
        protected static (MatchSide defender, MatchSide attacker, PositionDecideReason reason) GetMatchSidesOnStart(GameState gameState, params PlayerState[] playerStates)
        {
            var orderedPlayerStates = playerStates
                .OrderByDescending(WinPointsSelector)
                .ToArray();
            
            
            bool isSameWinPoints = WinPointsSelector(orderedPlayerStates[0]) == WinPointsSelector(orderedPlayerStates[1]);
            PositionDecideReason reason = isSameWinPoints
                ? PositionDecideReason.DecidedByRandom
                : PositionDecideReason.DecidedByWinPoints;
            
            if (isSameWinPoints)
            {
                // TODO: use PCG32
                Random random = new Random();
                orderedPlayerStates = orderedPlayerStates.OrderBy(_ => random.Next()).ToArray();
            }

            MatchSide defender = new MatchSide(orderedPlayerStates[0]);
            MatchSide attacker = new MatchSide(orderedPlayerStates[1]);
            
            return (defender, attacker, reason);

            int WinPointsSelector(PlayerState playerState)
            {
                return gameState.ScoreBoard.GetTotalWinPoints(playerState.Player);
            }
        }

        public static void CheckApplyBuffs(GameState gameState, IMatchContextEvent matchContextEvent, params MatchSide[] matchSide)
        {
            matchSide[0].CheckApplyCardBuffs(matchSide[1], gameState, out var oneSideBuffEvents);
            matchSide[1].CheckApplyCardBuffs(matchSide[0], gameState, out var otherSideBuffEvents);

            foreach (var oneSideBuffEvent in oneSideBuffEvents)
            {
                oneSideBuffEvent.RegisterEvent(matchContextEvent);
            }

            foreach (var otherSideBuffEvent in otherSideBuffEvents)
            {
                otherSideBuffEvent.RegisterEvent(matchContextEvent);
            }
        }
    }
    
    public class MatchContextEvent : IMatchContextEvent
    {
        public readonly int Round;
        public MatchResult Result { get; private set; }
        public bool MatchFinished { get; private set; }
        public List<IMatchEvent> MatchEvents { get; } = new List<IMatchEvent>();

        private MatchContextEvent(int round)
        {
            Round = round;
        }
        
        public void SetResult(IPlayer winPlayer, IPlayer losePlayer, MatchEndReason reason)
        {
            Result = new MatchResult(Round, winPlayer, losePlayer, reason);
            MatchFinished = true;
        }

        public static MatchContextEvent RunMatch(GameState currentState, params PlayerState[] playerStates)
        {
            MatchContextEvent matchContextEvent = new MatchContextEvent(currentState.Round);

            var (defender, attacker, reason) = IMatchContextEvent.GetMatchSidesOnStart(currentState, playerStates);
            defender.SetMatchState(MatchPosition.Defending);
            attacker.SetMatchState(MatchPosition.Attacking);

            MatchSnapshot matchSnapshotOnStart = new MatchSnapshot(currentState, defender, attacker);
            MatchStartEvent startEvent = new MatchStartEvent(attacker.Player, defender.Player, reason, matchSnapshotOnStart);
            startEvent.RegisterEvent(matchContextEvent);

            if (!defender.TryDraw(out Card drawnByDefender))
            {
                MatchFinishEvent finishByEmptyDeck = new MatchFinishEvent(
                    attacker.Player,
                    defender.Player,
                    MatchEndReason.EndByEmptyDeck
                );
                
                finishByEmptyDeck.RegisterEvent(matchContextEvent);
                return matchContextEvent;
            }

            RegisterDrawCardEvent(defender, matchContextEvent);

            CardEffectArgs defenderDrawnEffectArgs = new CardEffectArgs(
                EffectTriggerEvent.OnEnterFieldAsDefender,
                defender,
                attacker,
                currentState
            );
            
            drawnByDefender.CardEffect.CheckApplyEffect(defenderDrawnEffectArgs, matchContextEvent);
            if (matchContextEvent.MatchFinished)
            {
                return matchContextEvent;
            }
            
            IMatchContextEvent.CheckApplyBuffs(currentState, matchContextEvent, defender, attacker);

            while (true)
            {
                while (attacker.GetEffectivePower(defender, currentState) < defender.GetEffectivePower(attacker, currentState))
                {
                    if (!attacker.TryDraw(out Card drawnByAttacker))
                    {
                        MatchFinishEvent finishByEmptyDeck = new MatchFinishEvent(
                            defender.Player,
                            attacker.Player,
                            MatchEndReason.EndByEmptyDeck
                        );
                        
                        finishByEmptyDeck.RegisterEvent(matchContextEvent);
                        return matchContextEvent;
                    }

                    RegisterDrawCardEvent(attacker, matchContextEvent);
                    
                    CardEffectArgs attackerDrawnEffectArgs = new CardEffectArgs(
                        EffectTriggerEvent.OnEnterFieldAsAttacker,
                        attacker,
                        defender,
                        currentState
                    );
                    
                    drawnByAttacker.CardEffect.CheckApplyEffect(attackerDrawnEffectArgs, matchContextEvent);
                    if (matchContextEvent.MatchFinished)
                    {
                        return matchContextEvent;
                    }
                    
                    IMatchContextEvent.CheckApplyBuffs(currentState, matchContextEvent, defender, attacker);
                }

                SuccessAttackEvent successAttackEvent = new SuccessAttackEvent(attacker.Player);
                successAttackEvent.RegisterEvent(matchContextEvent);

                #region Put cards of defender from field to infirmary

                CardEffectArgs leaveFieldEffectArgs = new CardEffectArgs(
                    EffectTriggerEvent.OnLeaveField,
                    defender,
                    attacker,
                    currentState
                );
                
                CardEffectArgs enterInfirmaryEffectArgs = new CardEffectArgs(
                    EffectTriggerEvent.OnEnterInfirmary,
                    defender,
                    attacker,
                    currentState
                );

                while (defender.Field.Count > 0)
                {
                    // send card to infirmary in order according to designer's request
                    int indexOfField = 0;
                    Card cardToMove = defender.Field[indexOfField];
                    
                    defender.Field.RemoveAt(indexOfField);

                    bool isMovementReplaced = cardToMove.CardEffect.TryReplaceMovement(leaveFieldEffectArgs, matchContextEvent);
                    
                    // replace movement instead put to infirmary
                    if (isMovementReplaced)
                    {
                        IMatchContextEvent.CheckApplyBuffs(currentState, matchContextEvent, defender, attacker);
                        continue;
                    }
                    
                    defender.Infirmary.PutCard(cardToMove, out var infirmaryLocation);

                    CardLocation prevLocation = new FieldLocation(defender.Player, indexOfField);
                    CardMovementInfo movementInfo = new CardMovementInfo(prevLocation, infirmaryLocation);

                    SendToInfirmaryEvent sendInfirmaryEvent = new SendToInfirmaryEvent(defender.Player, movementInfo);
                    sendInfirmaryEvent.RegisterEvent(matchContextEvent);

                    if (!defender.Infirmary.IsSlotRemains)
                    {
                        MatchFinishEvent finishByFullOfInfirmary = new MatchFinishEvent(
                            attacker.Player,
                            defender.Player,
                            MatchEndReason.EndByFullOfInfirmary
                        );
                        
                        finishByFullOfInfirmary.RegisterEvent(matchContextEvent);
                        return matchContextEvent;
                    }
                    
                    cardToMove.CardEffect.CheckApplyEffect(enterInfirmaryEffectArgs, matchContextEvent);
                    if (matchContextEvent.MatchFinished)
                    {
                        return matchContextEvent;
                    }
                }

                #endregion
                
                IMatchContextEvent.CheckApplyBuffs(currentState, matchContextEvent, defender, attacker);
                
                // changes position between two players
                (defender, attacker) = (attacker, defender);
                defender.SetMatchState(MatchPosition.Defending);
                attacker.SetMatchState(MatchPosition.Attacking);

                #region Trigger defender's cards on field

                CardEffectArgs remainFieldEffectArgs = new CardEffectArgs(
                    EffectTriggerEvent.OnRemainField,
                    defender,
                    attacker,
                    currentState
                );
                
                foreach (var cardOnField in defender.Field.SkipLast(1))
                {
                    cardOnField.CardEffect.CheckApplyEffect(remainFieldEffectArgs, matchContextEvent);
                }

                CardEffectArgs switchToDefendEffectArgs = new CardEffectArgs(
                    EffectTriggerEvent.OnSwitchToDefend,
                    defender,
                    attacker,
                    currentState
                );
                
                defender.Field[^1].CardEffect.CheckApplyEffect(switchToDefendEffectArgs, matchContextEvent);

                #endregion
                
                IMatchContextEvent.CheckApplyBuffs(currentState, matchContextEvent, defender, attacker);

                MatchSnapshot matchSnapshotOnSwitch = new MatchSnapshot(currentState, defender, attacker);
                SwitchPositionEvent switchPositionEvent = new SwitchPositionEvent(defender.Player, attacker.Player, matchSnapshotOnSwitch);
                switchPositionEvent.RegisterEvent(matchContextEvent);
            }
        }

        private static void RegisterDrawCardEvent(MatchSide drawnSide, MatchContextEvent matchContextEvent)
        {
            CardLocation prevLocation = new DeckLocation(drawnSide.Player, drawnSide.Deck.Count);
            CardLocation curLocation = new FieldLocation(drawnSide.Player, drawnSide.Field.Count - 1);

            CardMovementInfo cardMovementInfo = new CardMovementInfo(prevLocation, curLocation);
            
            DrawCardToFieldEvent toFieldEvent = new DrawCardToFieldEvent(drawnSide.Player, cardMovementInfo);
            toFieldEvent.RegisterEvent(matchContextEvent);
        }
    }
}
