using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectABC.Data;

namespace ProjectABC.Core
{
    public class BattlePhase : IGamePhase
    {
        public async Task ExecutePhaseAsync(SimulationContext simulationContext)
        {
            GameState currentState = simulationContext.CurrentState;
            var matchingPairs = currentState.GetMatchingPairs();

            foreach (var (playerAState, playerBState) in matchingPairs)
            {
                MatchContextEvent matchContextEvent = RunMatch(playerAState, playerBState);
                matchContextEvent.Publish();
                
                simulationContext.CollectedEvents.Add(matchContextEvent);
                    
                PlayerState winnerState = simulationContext.CurrentState.GetPlayerState(matchContextEvent.Winner);
                WinPointOnRound winPointOnRound = new WinPointOnRound(currentState.Round);
                int winPoints = winPointOnRound.GetWinPoint();
                    
                winnerState.WinPoints += winPoints;
                    
                string message = $"플레이어 '{winnerState.Player.Name}'가 승리하여 {winPoints} 승점을 획득.\n"
                                 + $"총점 : {winnerState.WinPoints}";
                var gainWinPointContextEvent = new CommonConsoleEvent(message);
                gainWinPointContextEvent.Publish();
                
                simulationContext.CollectedEvents.Add(gainWinPointContextEvent);
            }

            await Task.WhenAll(simulationContext.GetTasksOfAllPlayersConfirmToProceed());
        }

        private static MatchContextEvent RunMatch(PlayerState playerAState, PlayerState playerBState)
        {
            MatchContextEvent matchContextEvent = new MatchContextEvent(playerAState.Player, playerBState.Player);
            
            var (defender, attacker) = GetMatchSidesOnStart(playerAState, playerBState);
            defender.SetMatchState(MatchState.Defending);
            attacker.SetMatchState(MatchState.Attacking);

            MatchStartEvent matchStartEvent = new MatchStartEvent(new MatchSnapshot(defender, attacker));
            matchStartEvent.RegisterEvent(matchContextEvent);

            if (!defender.TryDraw(out Card cardDrawnFromDefender))
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

            DrawCardEvent defenderDrawnEvent =
                new DrawCardEvent(defender.Player, new MatchSnapshot(defender, attacker));
            defenderDrawnEvent.RegisterEvent(matchContextEvent);
            
            while (true)
            {
                while (attacker.GetEffectivePower() < defender.GetEffectivePower())
                {
                    if (!attacker.TryDraw(out Card cardDrawnByAttacker))
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
                    
                    ComparePowerEvent comparePowerEvent = new ComparePowerEvent(new MatchSnapshot(defender, attacker));
                    comparePowerEvent.RegisterEvent(matchContextEvent);
                }

                defender.PutCardsToInfirmary(out List<Card> cardsToInfirmary);
                TryPutCardInfirmaryEvent putCardInfirmaryEvent = new TryPutCardInfirmaryEvent(
                    defender.Player,
                    cardsToInfirmary,
                    new MatchSnapshot(defender, attacker)
                );
                
                putCardInfirmaryEvent.RegisterEvent(matchContextEvent);
                
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
                
                SwitchPositionEvent switchPositionEvent = new SwitchPositionEvent(new MatchSnapshot(defender, attacker));
                switchPositionEvent.RegisterEvent(matchContextEvent);
            }
        }

        private static (MatchSide defender, MatchSide attacker) GetMatchSidesOnStart(PlayerState playerAState, PlayerState playerBState)
        { 
            List<PlayerState> orderedPlayerStates = new [] { playerAState, playerBState }
                .OrderByDescending(state => state.WinPoints)
                .ToList();
            
            bool allSameWinPoints = orderedPlayerStates.Select(state => state.WinPoints).Distinct().Count() <= 1;

            if (allSameWinPoints)
            {
                System.Random random = new System.Random();
                orderedPlayerStates = orderedPlayerStates.OrderBy(_ => random.Next()).ToList();
            }

            MatchSide defenderSide = new MatchSide(orderedPlayerStates[0]);
            MatchSide attackerSide = new MatchSide(orderedPlayerStates[1]);
            
            return (defenderSide, attackerSide);
        }
    }
    
    public class WinPointOnRound
    {
        private readonly List<Tuple<int, float>> _pointAndWeights;
        
        public WinPointOnRound(int round)
        {
            _pointAndWeights = Storage.Instance.WinPointData
                .Where(data => data.round == round)
                .Select(ElementSelector)
                .ToList();
        }

        private static Tuple<int, float> ElementSelector(WinPointData winPointData)
        {
            return new Tuple<int, float>(winPointData.winPoint, winPointData.weight);
        }

        public int GetWinPoint()
        {
            if (_pointAndWeights.Count == 0)
            {
                throw new InvalidOperationException("No win point data available");
            }

            // TODO : using PCG32RNG
            float pivot = 0.5f; //UnityEngine.Random.Range(0.0f, 1.0f);

            float totalWeight = _pointAndWeights.Sum(tuple => tuple.Item2);
            bool isTotalWeightLessThanZero = totalWeight <= 0;

            if (isTotalWeightLessThanZero)
            {
                var (firstWinPoint, _) = _pointAndWeights.First();
                
                return firstWinPoint;
            }
            
            float sumOfRatio = 0.0f;

            foreach (var (winPoint, weight) in _pointAndWeights)
            {
                float weightRatio = weight / totalWeight;
                sumOfRatio += weightRatio;

                if (sumOfRatio > pivot)
                {
                    return winPoint;
                }
            }

            var (lastWinPoint, _) = _pointAndWeights.Last();
            
            return lastWinPoint;
        }
    }
}