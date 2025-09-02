using System.Collections.Generic;
using System.Linq;


namespace ProjectABC.Core
{
    public class MatchContextEvent : IContextEvent
    {
        public readonly int Round;
        public IPlayer WinPlayer { get; private set; } = null;
        public IPlayer LostPlayer { get; private set; } = null;
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
            return player == WinPlayer || player == LostPlayer;
        }

        public void SetResult(IPlayer winPlayer, IPlayer lostPlayer)
        {
            WinPlayer = winPlayer;
            LostPlayer = lostPlayer;
        }

        public static MatchContextEvent RunMatch(int round, ScoreBoard scoreBoard, params PlayerState[] playerStates)
        {
            MatchContextEvent matchContextEvent = new MatchContextEvent(round);
            
            var (defender, attacker) = GetMatchSidesOnStart(round, scoreBoard, playerStates[0], playerStates[1]);
            defender.SetMatchState(MatchState.Defending);
            attacker.SetMatchState(MatchState.Attacking);

            MatchStartEvent matchStartEvent = new MatchStartEvent(new MatchSnapshot(defender, attacker));
            matchStartEvent.RegisterEvent(matchContextEvent);

            if (!defender.TryDraw(out Card drawnCardFromDefender))
            {
                // set attacker winner and return events
                MatchFinishEvent matchFinishEvent = new MatchFinishEvent(
                    attacker.Player,
                    MatchFinishEvent.MatchEndReason.EndByEmptyHand,
                    new MatchSnapshot(attacker, defender)
                );
                
                matchFinishEvent.RegisterEvent(matchContextEvent);
                
                return matchContextEvent;
            }

            DrawCardEvent defenderDrawnEvent = new DrawCardEvent(defender.Player, new MatchSnapshot(defender, attacker));
            defenderDrawnEvent.RegisterEvent(matchContextEvent);

            drawnCardFromDefender.CardEffect.TryApplyEffect(
                EffectTriggerEvent.OnEnterFieldAsDefender,
                defender,
                attacker,
                out var applyCardEffectOfDefenderDrawnEvent
            );
            
            applyCardEffectOfDefenderDrawnEvent?.RegisterEvent(matchContextEvent);
            
            while (true)
            {
                while (attacker.GetEffectivePower() < defender.GetEffectivePower())
                {
                    if (!attacker.TryDraw(out Card drawnCardFromAttacker))
                    {
                        // set defender winner and return events
                        MatchFinishEvent matchFinishEvent = new MatchFinishEvent(
                            defender.Player,
                            MatchFinishEvent.MatchEndReason.EndByEmptyHand,
                            new MatchSnapshot(attacker, defender)
                        );
                
                        matchFinishEvent.RegisterEvent(matchContextEvent);
                        
                        return matchContextEvent;
                    }
                    
                    DrawCardEvent attackerDrawnEvent = new DrawCardEvent(attacker.Player, new MatchSnapshot(defender, attacker));
                    attackerDrawnEvent.RegisterEvent(matchContextEvent);

                    drawnCardFromAttacker.CardEffect.TryApplyEffect(
                        EffectTriggerEvent.OnEnterFieldAsAttacker,
                        attacker,
                        defender,
                        out var applyCardEffectOfAttackerDrawnEvent
                    );
                    
                    applyCardEffectOfAttackerDrawnEvent?.RegisterEvent(matchContextEvent);
                    
                    attacker.CheckApplyCardBuffs();
                    
                    ComparePowerEvent comparePowerEvent = new ComparePowerEvent(new MatchSnapshot(defender, attacker));
                    comparePowerEvent.RegisterEvent(matchContextEvent);
                }

                PutCardsOfDefenderFieldsToInfirmary(defender, attacker, matchContextEvent);
                
                if (!defender.Infirmary.IsSlotRemains)
                {
                    // set attacker winner and return events
                    MatchFinishEvent matchFinishEvent = new MatchFinishEvent(
                        attacker.Player,
                        MatchFinishEvent.MatchEndReason.EndByFullOfInfirmary,
                        new MatchSnapshot(attacker, defender)
                    );
                    
                    matchFinishEvent.RegisterEvent(matchContextEvent);
                    
                    return matchContextEvent;
                }
                
                // changes position between two players
                (defender, attacker) = (attacker, defender);
                defender.SetMatchState(MatchState.Defending);
                attacker.SetMatchState(MatchState.Attacking);
                
                TriggerDefenderFieldCards(defender, attacker, matchContextEvent);
                defender.CheckApplyCardBuffs();
                
                SwitchPositionEvent switchPositionEvent = new SwitchPositionEvent(new MatchSnapshot(defender, attacker));
                switchPositionEvent.RegisterEvent(matchContextEvent);
            }
        }
        
        private static (MatchSide defender, MatchSide attacker) GetMatchSidesOnStart(int round, ScoreBoard scoreBoard, params PlayerState[] playerStates)
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
                return scoreBoard.GetTotalWinPoints(playerState.Player, round);
            }
        }

        private static void PutCardsOfDefenderFieldsToInfirmary(MatchSide defender, MatchSide attacker, MatchContextEvent matchContextEvent)
        {
            List<Card> cardsToInfirmary = new List<Card>();
            
            // replace movement instead put to infirmary
            for (int i = defender.Field.Count - 1; i >= 0; i--)
            {
                Card fieldCard = defender.Field[i];
                bool isMovementReplaced = fieldCard.CardEffect.TryReplaceMovement(
                    EffectTriggerEvent.OnLeaveField,
                    defender,
                    out IMatchEvent movementReplaceEvent
                );

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
                card.CardEffect.TryApplyEffect(
                    EffectTriggerEvent.OnEnterInfirmary,
                    defender,
                    attacker,
                    out IMatchEvent applyCardEffectOfInfirmaryCardEvent
                );
                
                applyCardEffectOfInfirmaryCardEvent?.RegisterEvent(matchContextEvent);
            }

            defender.CheckApplyCardBuffs();
        }

        private static void TriggerDefenderFieldCards(MatchSide defender, MatchSide attacker, MatchContextEvent matchContextEvent)
        {
            foreach (var card in defender.Field.SkipLast(1))
            {
                card.CardEffect.TryApplyEffect(
                    EffectTriggerEvent.OnRemainField,
                    defender,
                    attacker,
                    out IMatchEvent applyCardEffectOfRemainFieldEvent
                );
                
                applyCardEffectOfRemainFieldEvent?.RegisterEvent(matchContextEvent);
            }

            defender.Field[^1].CardEffect.TryApplyEffect(
                EffectTriggerEvent.OnSwitchToDefend,
                defender,
                attacker,
                out IMatchEvent applyCardEffectOfSwitchDefendEvent
            );
            
            applyCardEffectOfSwitchDefendEvent?.RegisterEvent(matchContextEvent);
        }
    }
}
