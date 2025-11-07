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

        protected static void CheckApplyBuffs(GameState gameState, IMatchContextEvent matchContextEvent, params MatchSide[] matchSide)
        {
            matchSide[0].CheckApplyCardBuffs(matchSide[1], gameState);
            matchSide[1].CheckApplyCardBuffs(matchSide[0], gameState);
            
            // TODO: register match event if need to announce applying buffs
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
            defender.SetMatchState(MatchState.Defending);
            attacker.SetMatchState(MatchState.Attacking);

            MatchStartEvent startEvent = new MatchStartEvent(attacker.Player, defender.Player, reason);
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

            MatchSnapshot snapshotForDefenderDraw = new MatchSnapshot(currentState, defender, attacker);
            snapshotForDefenderDraw.TryGetMatchSideSnapshot(defender.Player, out var defenderSideSnapshot);
            var defenderDrawnCard = defenderSideSnapshot.Field[^1];

            CardDrawEvent defenderDrawEvent = new CardDrawEvent(defender.Player, defenderDrawnCard, snapshotForDefenderDraw);
            defenderDrawEvent.RegisterEvent(matchContextEvent);

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

                    MatchSnapshot snapShotForAttackerDraw = new MatchSnapshot(currentState, defender, attacker);
                    snapShotForAttackerDraw.TryGetMatchSideSnapshot(attacker.Player, out var attackerSideSnapshot);
                    var attackerDrawnCard = attackerSideSnapshot.Field[^1];

                    CardDrawEvent attackerDrawEvent = new CardDrawEvent(attacker.Player, attackerDrawnCard, snapShotForAttackerDraw);
                    attackerDrawEvent.RegisterEvent(matchContextEvent);
                    
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

                SuccessAttackEvent successAttackEvent = new SuccessAttackEvent();
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
                    Card cardToMove = defender.Field[^1];
                    defender.Field.Remove(cardToMove);

                    bool isMovementReplaced = cardToMove.CardEffect.TryReplaceMovement(leaveFieldEffectArgs, matchContextEvent);
                    
                    // replace movement instead put to infirmary
                    if (isMovementReplaced)
                    {
                        IMatchContextEvent.CheckApplyBuffs(currentState, matchContextEvent, defender, attacker);
                        continue;
                    }
                    
                    defender.Infirmary.PutCard(cardToMove);
                    
                    MatchSnapshot snapshotForPutInfirmary = new MatchSnapshot(currentState, defender, attacker);
                    // snapshotForPutInfirmary.TryGetMatchSideSnapshot(defender.Player, out var defenderSideSnapshot);
                }

                #endregion
            }
        }
    }
}
