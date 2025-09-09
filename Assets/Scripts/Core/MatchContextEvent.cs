using System.Collections.Generic;
using System.Linq;


namespace ProjectABC.Core
{
    public enum MatchEndReason
    {
        EndByEmptyDeck,
        EndByFullOfInfirmary
    }
    
    public class MatchContextEvent : IContextEvent
    {
        public readonly int Round;

        public MatchResult Result { get; private set; }
        public bool MatchFinished { get; private set; } = false;
        
        public List<IMatchEvent> MatchEvents { get; } = new List<IMatchEvent>();

        public IReadOnlyDictionary<IPlayer, MatchSideSnapshot> LastMatchSideSnapShots => MatchEvents[^1]
            .Snapshot
            .MatchSideSnapShots;

        private MatchContextEvent(int round)
        {
            Round = round;
        }

        public bool IsParticipants(IPlayer player)
        {
            return Result.IsParticipants(player);
        }

        public void SetResult(IPlayer winPlayer, IPlayer losePlayer, MatchEndReason reason)
        {
            Result = new MatchResult(Round, winPlayer, losePlayer, reason);
            MatchFinished = true;
        }

        public static MatchContextEvent RunMatch(GameState currentState, params PlayerState[] playerStates)
        {
            MatchContextEvent matchContextEvent = new MatchContextEvent(currentState.Round);
                                                                                                                     
            var (defender, attacker) = GetMatchSidesOnStart(currentState.ScoreBoard, playerStates[0], playerStates[1]);
            defender.SetMatchState(MatchState.Defending);
            attacker.SetMatchState(MatchState.Attacking);

            MatchStartEvent matchStartEvent = new MatchStartEvent(new MatchSnapshot(defender, attacker));
            matchStartEvent.RegisterEvent(matchContextEvent);

            if (!defender.TryDraw(out Card drawnCardFromDefender))
            {
                // set attacker winner and return events
                MatchFinishEvent matchFinishEvent = new MatchFinishEvent(
                    attacker.Player,
                    MatchEndReason.EndByEmptyDeck,
                    new MatchSnapshot(attacker, defender)
                );
                
                matchFinishEvent.RegisterEvent(matchContextEvent);
                return matchContextEvent;
            }

            DrawCardEvent defenderDrawnEvent = new DrawCardEvent(defender.Player, new MatchSnapshot(defender, attacker));
            defenderDrawnEvent.RegisterEvent(matchContextEvent);

            CardEffectArgs defenderDrawnEffectArgs = new CardEffectArgs(
                EffectTriggerEvent.OnEnterFieldAsDefender,
                defender,
                attacker,
                currentState
            );
            
            drawnCardFromDefender.CardEffect.CheckApplyEffect(defenderDrawnEffectArgs, matchContextEvent);
            if (matchContextEvent.MatchFinished)
            {
                return matchContextEvent;
            }
            
            CheckApplyBuffs(defender, attacker, currentState, matchContextEvent);
            
            while (true)
            {
                while (attacker.GetEffectivePower(defender, currentState) < defender.GetEffectivePower(attacker, currentState))
                {
                    if (!attacker.TryDraw(out Card drawnCardFromAttacker))
                    {
                        // set defender winner and return events
                        MatchFinishEvent matchFinishEvent = new MatchFinishEvent(
                            defender.Player,
                            MatchEndReason.EndByEmptyDeck,
                            new MatchSnapshot(attacker, defender)
                        );
                
                        matchFinishEvent.RegisterEvent(matchContextEvent);
                        return matchContextEvent;
                    }
                    
                    DrawCardEvent attackerDrawnEvent = new DrawCardEvent(attacker.Player, new MatchSnapshot(defender, attacker));
                    attackerDrawnEvent.RegisterEvent(matchContextEvent);

                    CardEffectArgs attackerDrawnEffectArgs = new CardEffectArgs(
                        EffectTriggerEvent.OnEnterFieldAsAttacker,
                        attacker,
                        defender,
                        currentState
                    );
                    
                    drawnCardFromAttacker.CardEffect.CheckApplyEffect(attackerDrawnEffectArgs, matchContextEvent);
                    if (matchContextEvent.MatchFinished)
                    {
                        return matchContextEvent;
                    }
                    
                    CheckApplyBuffs(defender, attacker, currentState, matchContextEvent);
                    
                    ComparePowerEvent comparePowerEvent = new ComparePowerEvent(new MatchSnapshot(defender, attacker));
                    comparePowerEvent.RegisterEvent(matchContextEvent);
                }

                PutCardsOfDefenderFieldsToInfirmary(defender, attacker, currentState, matchContextEvent);
                if (matchContextEvent.MatchFinished)
                {
                    return matchContextEvent;
                }
                
                CheckApplyBuffs(defender, attacker, currentState, matchContextEvent);
                
                // changes position between two players
                (defender, attacker) = (attacker, defender);
                defender.SetMatchState(MatchState.Defending);
                attacker.SetMatchState(MatchState.Attacking);
                
                TriggerDefenderFieldCards(defender, attacker, currentState, matchContextEvent);
                
                SwitchPositionEvent switchPositionEvent = new SwitchPositionEvent(new MatchSnapshot(defender, attacker));
                switchPositionEvent.RegisterEvent(matchContextEvent);
                
                CheckApplyBuffs(defender, attacker, currentState, matchContextEvent);
            }
        }
        
        private static (MatchSide defender, MatchSide attacker) GetMatchSidesOnStart(ScoreBoard scoreBoard, params PlayerState[] playerStates)
        { 
            List<PlayerState> orderedPlayerStates = playerStates
                .OrderByDescending(WinPointsSelector)
                .ToList();
            
            bool allSameWinPoints = WinPointsSelector(orderedPlayerStates[0]) == WinPointsSelector(orderedPlayerStates[1]);
            if (allSameWinPoints)
            {
                System.Random random = new System.Random();
                orderedPlayerStates = orderedPlayerStates.OrderBy(_ => random.Next()).ToList();
            }

            MatchSide defenderSide = new MatchSide(orderedPlayerStates[0]);
            MatchSide attackerSide = new MatchSide(orderedPlayerStates[1]);
            
            return (defenderSide, attackerSide);

            int WinPointsSelector(PlayerState playerState)
            {
                return scoreBoard.GetTotalWinPoints(playerState.Player);
            }
        }

        private static void PutCardsOfDefenderFieldsToInfirmary(MatchSide defender, MatchSide attacker, GameState currentState, MatchContextEvent matchContextEvent)
        {
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
            
            while (defender.Field.Count == 0)
            {
                Card cardToMove = defender.Field[^1];
                defender.Field.Remove(cardToMove);
                
                bool isMovementReplaced = cardToMove.CardEffect.TryReplaceMovement(leaveFieldEffectArgs, matchContextEvent);
                
                // replace movement instead put to infirmary
                if (isMovementReplaced)
                {
                    CheckApplyBuffs(defender, attacker, currentState, matchContextEvent);
                    continue;
                }
                
                // put cards from defender field to infirmary
                defender.Infirmary.PutCard(cardToMove);
                
                TryPutCardInfirmaryEvent putCardInfirmaryEvent = new TryPutCardInfirmaryEvent(
                    defender.Player,
                    cardToMove,
                    new MatchSnapshot(defender, attacker)
                );
                
                putCardInfirmaryEvent.RegisterEvent(matchContextEvent);

                if (!defender.Infirmary.IsSlotRemains)
                {
                    MatchFinishEvent matchFinishEvent = new MatchFinishEvent(
                        attacker.Player,
                        MatchEndReason.EndByFullOfInfirmary,
                        new MatchSnapshot(defender, attacker)
                    );
                    
                    matchFinishEvent.RegisterEvent(matchContextEvent);
                    return;
                }
                
                cardToMove.CardEffect.CheckApplyEffect(enterInfirmaryEffectArgs, matchContextEvent);
                if (matchContextEvent.MatchFinished)
                {
                    return;
                }
            }
        }

        public static void CheckApplyBuffs(MatchSide playerASide, MatchSide playerBSide, GameState currentState, MatchContextEvent matchContextEvent)
        {
            playerASide.CheckApplyCardBuffs(playerBSide, currentState);
            playerBSide.CheckApplyCardBuffs(playerASide, currentState);
            
            CheckApplyBuffEvent checkApplyBuffEvent = new CheckApplyBuffEvent(new MatchSnapshot(playerASide, playerBSide));
            checkApplyBuffEvent.RegisterEvent(matchContextEvent);
        }

        private static void TriggerDefenderFieldCards(MatchSide defender, MatchSide attacker, GameState currentState, MatchContextEvent matchContextEvent)
        {
            foreach (var card in defender.Field.SkipLast(1))
            {
                CardEffectArgs remainFieldEffectArgs = new CardEffectArgs(
                    EffectTriggerEvent.OnRemainField,
                    defender,
                    attacker,
                    currentState
                );
                
                card.CardEffect.CheckApplyEffect(remainFieldEffectArgs, matchContextEvent);
            }

            CardEffectArgs switchToDefendEffectArgs = new CardEffectArgs(
                EffectTriggerEvent.OnSwitchToDefend,
                defender,
                attacker,
                currentState
            );
            
            defender.Field[^1].CardEffect.CheckApplyEffect(switchToDefendEffectArgs, matchContextEvent);
        }
    }
}
