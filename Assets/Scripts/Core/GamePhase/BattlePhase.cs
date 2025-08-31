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
            var matchingPairs = simulationContext.CurrentState.GetMatchingPairs();
            
            GameState currentState = simulationContext.CurrentState;
            int round = currentState.Round;

            foreach (var (playerAState, playerBState) in matchingPairs)
            {
                // TODO : run match include gained win points
                MatchContextEvent matchContextEvent = MatchContextEvent.RunMatch(
                    round,
                    currentState.ScoreBoard,
                    playerAState, playerBState
                );
                
                matchContextEvent.Publish();
                
                simulationContext.CollectedEvents.Add(matchContextEvent);
                
                WinPointOnRound winPointOnRound = new WinPointOnRound(currentState.Round);
                int roundWinPoints = winPointOnRound.GetWinPoint();
                
                RoundScore winningScore = new RoundScore(
                    round,
                    matchContextEvent.WinPlayer,
                    matchContextEvent.LostPlayer,
                    matchContextEvent.LastMatchSideSnapShots[matchContextEvent.WinPlayer].GainedWinPointsOnMatch + roundWinPoints,
                    RoundResult.Win
                );

                RoundScore losingScore = new RoundScore(
                    round,
                    matchContextEvent.LostPlayer,
                    matchContextEvent.WinPlayer,
                    matchContextEvent.LastMatchSideSnapShots[matchContextEvent.LostPlayer].GainedWinPointsOnMatch,
                    RoundResult.Lose
                );
                
                currentState.ScoreBoard.RegisterRoundScores(winningScore, losingScore);
                
                // TODO: publish ContextEvent here for announce win points what winner gained on round and total win points
            }

            await Task.WhenAll(simulationContext.GetTasksOfAllPlayersConfirmToProceed());
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