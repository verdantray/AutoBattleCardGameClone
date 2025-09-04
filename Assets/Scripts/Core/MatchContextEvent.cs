using System.Collections.Generic;
using System.Linq;


namespace ProjectABC.Core
{
    public enum MatchEndReason
    {
        EndByEmptyHand,
        EndByFullOfInfirmary
    }
    
    public class MatchContextEvent : IContextEvent
    {
        public readonly int Round;
        public MatchResult Result { get; private set; }
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
                    MatchEndReason.EndByEmptyHand,
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
            
            drawnCardFromDefender.CardEffect.TryApplyEffect(defenderDrawnEffectArgs, out var applyCardEffectOfDefenderDrawnEvent);
            applyCardEffectOfDefenderDrawnEvent?.RegisterEvent(matchContextEvent);
            
            defender.CheckApplyCardBuffs();
            
            #region check infirmary slot remains after every card effect applied, because of card effect can put cards to infirmary

            if (!defender.Infirmary.IsSlotRemains)
            {
                // set attacker winner and return events
                MatchFinishEvent matchFinishEvent = new MatchFinishEvent(
                    attacker.Player,
                    MatchEndReason.EndByFullOfInfirmary,
                    new MatchSnapshot(attacker, defender)
                );
                    
                matchFinishEvent.RegisterEvent(matchContextEvent);
                return matchContextEvent;
            }
            
            if (!attacker.Infirmary.IsSlotRemains)
            {
                // set defender winner and return events
                MatchFinishEvent matchFinishEvent = new MatchFinishEvent(
                    defender.Player,
                    MatchEndReason.EndByFullOfInfirmary,
                    new MatchSnapshot(attacker, defender)
                );
                    
                matchFinishEvent.RegisterEvent(matchContextEvent);
                return matchContextEvent;
            }

            #endregion
            
            while (true)
            {
                while (attacker.GetEffectivePower(defender) < defender.GetEffectivePower(attacker))
                {
                    if (!attacker.TryDraw(out Card drawnCardFromAttacker))
                    {
                        // set defender winner and return events
                        MatchFinishEvent matchFinishEvent = new MatchFinishEvent(
                            defender.Player,
                            MatchEndReason.EndByEmptyHand,
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
                    
                    drawnCardFromAttacker.CardEffect.TryApplyEffect(attackerDrawnEffectArgs, out var applyCardEffectOfAttackerDrawnEvent);
                    applyCardEffectOfAttackerDrawnEvent?.RegisterEvent(matchContextEvent);
                    
                    attacker.CheckApplyCardBuffs();
                    
                    #region check infirmary slot remains after every card effect applied, because of card effect can put cards to infirmary

                    if (!defender.Infirmary.IsSlotRemains)
                    {
                        // set attacker winner and return events
                        MatchFinishEvent matchFinishEvent = new MatchFinishEvent(
                            attacker.Player,
                            MatchEndReason.EndByFullOfInfirmary,
                            new MatchSnapshot(attacker, defender)
                        );
                    
                        matchFinishEvent.RegisterEvent(matchContextEvent);
                        return matchContextEvent;
                    }
            
                    if (!attacker.Infirmary.IsSlotRemains)
                    {
                        // set defender winner and return events
                        MatchFinishEvent matchFinishEvent = new MatchFinishEvent(
                            defender.Player,
                            MatchEndReason.EndByFullOfInfirmary,
                            new MatchSnapshot(attacker, defender)
                        );
                    
                        matchFinishEvent.RegisterEvent(matchContextEvent);
                        return matchContextEvent;
                    }

                    #endregion
                    
                    ComparePowerEvent comparePowerEvent = new ComparePowerEvent(new MatchSnapshot(defender, attacker));
                    comparePowerEvent.RegisterEvent(matchContextEvent);
                }

                PutCardsOfDefenderFieldsToInfirmary(defender, attacker, currentState, matchContextEvent);
                
                if (!defender.Infirmary.IsSlotRemains)
                {
                    // set attacker winner and return events
                    MatchFinishEvent matchFinishEvent = new MatchFinishEvent(
                        attacker.Player,
                        MatchEndReason.EndByFullOfInfirmary,
                        new MatchSnapshot(attacker, defender)
                    );
                    
                    matchFinishEvent.RegisterEvent(matchContextEvent);
                    
                    return matchContextEvent;
                }
                
                // changes position between two players
                (defender, attacker) = (attacker, defender);
                defender.SetMatchState(MatchState.Defending);
                attacker.SetMatchState(MatchState.Attacking);
                
                TriggerDefenderFieldCards(defender, attacker, currentState, matchContextEvent);
                defender.CheckApplyCardBuffs();
                
                SwitchPositionEvent switchPositionEvent = new SwitchPositionEvent(new MatchSnapshot(defender, attacker));
                switchPositionEvent.RegisterEvent(matchContextEvent);
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
            List<Card> cardsToInfirmary = new List<Card>();
            
            // replace movement instead put to infirmary
            for (int i = defender.Field.Count - 1; i >= 0; i--)
            {
                Card fieldCard = defender.Field[i];
                CardEffectArgs leaveFieldEffectArgs = new CardEffectArgs(
                    EffectTriggerEvent.OnLeaveField,
                    defender,
                    attacker,
                    currentState
                );
                
                bool isMovementReplaced = fieldCard.CardEffect.TryReplaceMovement(leaveFieldEffectArgs, out IMatchEvent movementReplaceEvent);
                if (!isMovementReplaced)
                {
                    cardsToInfirmary.Add(fieldCard);
                    defender.Field.Remove(fieldCard);
                    
                    continue;
                }
                
                movementReplaceEvent.RegisterEvent(matchContextEvent);
            }
            
            defender.CheckApplyCardBuffs();

            if (cardsToInfirmary.Count == 0)
            {
                return;
            }
            
            // put cards from defender field to infirmary
            defender.Infirmary.PutCards(cardsToInfirmary);
            
            TryPutCardInfirmaryEvent putCardInfirmaryEvent = new TryPutCardInfirmaryEvent(
                defender.Player,
                cardsToInfirmary,
                new MatchSnapshot(defender, attacker)
            );
            
            putCardInfirmaryEvent.RegisterEvent(matchContextEvent);
            
            foreach (Card card in cardsToInfirmary)
            {
                CardEffectArgs enterInfirmaryEffectArgs = new CardEffectArgs(
                    EffectTriggerEvent.OnEnterInfirmary,
                    defender,
                    attacker,
                    currentState
                );
                
                card.CardEffect.TryApplyEffect(enterInfirmaryEffectArgs, out IMatchEvent applyCardEffectOfInfirmaryCardEvent);
                applyCardEffectOfInfirmaryCardEvent?.RegisterEvent(matchContextEvent);
            }

            defender.CheckApplyCardBuffs();
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
                
                card.CardEffect.TryApplyEffect(remainFieldEffectArgs, out IMatchEvent applyCardEffectOfRemainFieldEvent);
                applyCardEffectOfRemainFieldEvent?.RegisterEvent(matchContextEvent);
            }

            CardEffectArgs switchToDefendEffectArgs = new CardEffectArgs(
                EffectTriggerEvent.OnSwitchToDefend,
                defender,
                attacker,
                currentState
            );
            
            defender.Field[^1].CardEffect.TryApplyEffect(switchToDefendEffectArgs, out IMatchEvent applyCardEffectOfSwitchDefendEvent);
            applyCardEffectOfSwitchDefendEvent?.RegisterEvent(matchContextEvent);
        }
    }
}
