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
                MatchFlowContextEvent matchFlowContextEvent = RunMatch(playerAState, playerBState);
                matchFlowContextEvent.Publish();
                
                simulationContext.CollectedEvents.Add(matchFlowContextEvent);
                    
                PlayerState winnerState = simulationContext.CurrentState.GetPlayerState(matchFlowContextEvent.Winner);
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

        private static MatchFlowContextEvent RunMatch(PlayerState playerAState, PlayerState playerBState)
        {
            MatchFlowContextEvent matchFlowContextEvent = new MatchFlowContextEvent(playerAState.Player, playerBState.Player);
            
            var (defender, attacker) = GetMatchSidesOnStart(playerAState, playerBState);
            defender.SetMatchState(MatchState.Defending);
            attacker.SetMatchState(MatchState.Attacking);
            
            MatchFlowSnapshot startSnapShot = new MatchFlowSnapshot(defender, attacker);
            matchFlowContextEvent.AddEvent(new MatchStartEvent(defender.Player, attacker.Player, startSnapShot));

            if (!defender.TryDraw())
            {
                // set attacker winner and return events
                matchFlowContextEvent.SetWinner(attacker.Player);
                    
                MatchFlowSnapshot finishSnapShot = new MatchFlowSnapshot(defender, attacker);
                matchFlowContextEvent.AddEvent(new MatchFinishEvent(attacker.Player, defender.Player, MatchFinishEvent.MatchEndReason.EndByEmptyHand, finishSnapShot));
                
                return matchFlowContextEvent;
            }
            
            MatchFlowSnapshot defenderDrawSnapshot = new MatchFlowSnapshot(defender, attacker);
            matchFlowContextEvent.AddEvent(new DrawCardEvent(defender.Player, defenderDrawSnapshot));
            
            while (true)
            {
                while (attacker.GetEffectivePower() < defender.GetEffectivePower())
                {
                    if (attacker.Field.Count > 0 && defender.Field.Count > 0)
                    {
                        MatchFlowSnapshot compareSnapshot = new MatchFlowSnapshot(defender, attacker);
                        matchFlowContextEvent.AddEvent(new ComparePowerEvent(defender, attacker, compareSnapshot));
                    }
                    
                    if (!attacker.TryDraw())
                    {
                        // set defender winner and return events
                        matchFlowContextEvent.SetWinner(defender.Player);
                        
                        MatchFlowSnapshot finishSnapshot = new MatchFlowSnapshot(defender, attacker);
                        matchFlowContextEvent.AddEvent(new MatchFinishEvent(defender.Player, attacker.Player, MatchFinishEvent.MatchEndReason.EndByEmptyHand, finishSnapshot));
                        
                        return matchFlowContextEvent;
                    }
                    
                    MatchFlowSnapshot attacherDrawSnapshot = new MatchFlowSnapshot(defender, attacker);
                    matchFlowContextEvent.AddEvent(new DrawCardEvent(attacker.Player, attacherDrawSnapshot));
                }
                
                MatchFlowSnapshot infirmarySnapshot = new MatchFlowSnapshot(defender, attacker);
                matchFlowContextEvent.AddEvent(new TryPutCardInfirmaryEvent(defender, infirmarySnapshot));
                
                if (!defender.TryPutCardFieldToInfirmary(out int _))
                {
                    // set attacker winner and return events
                    matchFlowContextEvent.SetWinner(attacker.Player);
                    
                    MatchFlowSnapshot finishSnapshot = new MatchFlowSnapshot(defender, attacker);
                    matchFlowContextEvent.AddEvent(new MatchFinishEvent(attacker.Player, defender.Player, MatchFinishEvent.MatchEndReason.EndByFullOfInfirmary, finishSnapshot));
                    
                    return matchFlowContextEvent;
                }
                
                // changes position between two players
                (defender, attacker) = (attacker, defender);
                defender.SetMatchState(MatchState.Defending);
                attacker.SetMatchState(MatchState.Attacking);
                
                MatchFlowSnapshot switchSnapshot = new MatchFlowSnapshot(defender, attacker);
                matchFlowContextEvent.AddEvent(new SwitchPositionEvent(defender.Player, attacker.Player, switchSnapshot));
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