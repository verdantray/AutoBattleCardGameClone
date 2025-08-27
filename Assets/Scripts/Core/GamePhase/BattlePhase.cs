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
                MatchResult matchResult = RunMatch(playerAState, playerBState);
                var matchFlowContextEvent = new MatchFlowConsoleEvent(matchResult.MatchEvents);
                matchFlowContextEvent.Publish();
                
                simulationContext.CollectedEvents.Add(matchFlowContextEvent);
                    
                PlayerState winnerState = simulationContext.CurrentState.GetPlayerState(matchResult.Winner);
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

        private static MatchResult RunMatch(PlayerState playerAState, PlayerState playerBState)
        {
            MatchResult matchResult = new MatchResult();
            
            var (defender, attacker) = GetMatchSidesOnStart(playerAState, playerBState);
            defender.HasFlag = true;
            attacker.HasFlag = false;
            
            matchResult.AddEvent(new MatchStartConsoleEvent(defender.Player, attacker.Player));

            if (!defender.TryDraw())
            {
                // set attacker winner and return events
                matchResult.SetWinner(attacker.Player);
                matchResult.AddEvent(new MatchFinishConsoleEvent(attacker, defender, MatchFinishConsoleEvent.MatchEndReason.EndByEmptyHand));
                
                return matchResult;
            }
            
            matchResult.AddEvent(new DrawCardConsoleEvent(defender));

            while (true)
            {
                matchResult.AddEvent(new ComparePowerConsoleEvent(defender, attacker));
                
                while (attacker.GetPower() < defender.GetPower())
                {
                    if (!attacker.TryDraw())
                    {
                        // set defender winner and return events
                        matchResult.SetWinner(defender.Player);
                        matchResult.AddEvent(new MatchFinishConsoleEvent(defender, attacker, MatchFinishConsoleEvent.MatchEndReason.EndByEmptyHand));
                        
                        return matchResult;
                    }
                    
                    matchResult.AddEvent(new DrawCardConsoleEvent(attacker));
                }
                
                matchResult.AddEvent(new TryPutCardInfirmaryConsoleEvent(defender));
                
                if (!defender.TryPutCardFieldToInfirmary(out int _))
                {
                    // set attacker winner and return events
                    matchResult.SetWinner(attacker.Player);
                    matchResult.AddEvent(new MatchFinishConsoleEvent(attacker, defender, MatchFinishConsoleEvent.MatchEndReason.EndByFullOfInfirmary));
                    
                    return matchResult;
                }
                
                // changes position between two players
                (defender, attacker) = (attacker, defender);
                defender.HasFlag = true;
                attacker.HasFlag = false;
                
                matchResult.AddEvent(new SwitchPositionConsoleEvent(defender.Player, attacker.Player));
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