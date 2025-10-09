using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectABC.Data;

namespace ProjectABC.Core
{
    public class BattleTestPhase : IGamePhase
    {
        public async Task ExecutePhaseAsync(SimulationContext simulationContext)
        {
            var matchingPairs = simulationContext.CurrentState.GetMatchingPairs();
            
            GameState currentState = simulationContext.CurrentState;

            foreach (var (playerAState, playerBState) in matchingPairs)
            {
                MatchContextConsoleEvent matchContextConsoleEvent = MatchContextConsoleEvent.RunMatch(currentState, playerAState, playerBState);
                
                matchContextConsoleEvent.Publish();
                simulationContext.CollectedEvents.Add(matchContextConsoleEvent);
                
                currentState.MatchResults.AddMatchResult(matchContextConsoleEvent.Result);
                
                WinPointOnRound winPointOnRound = new WinPointOnRound(matchContextConsoleEvent.Round);
                int roundWinPoints = winPointOnRound.GetWinPoint();
                
                ScoreEntry winnerEntry = new ScoreEntry(roundWinPoints, ScoreEntry.ScoreReason.ScoreByMatchWin);
                currentState.ScoreBoard.RegisterScoreEntry(matchContextConsoleEvent.Result.Winner, winnerEntry);
                
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
            double pivot = new Random().NextDouble();

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