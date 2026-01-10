using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectABC.Core;
using UnityEngine;

namespace ProjectABC.Engine
{
    [CreateAssetMenu(fileName = nameof(MatchPhase), menuName = "Scripting/ScriptableObject Script Menu/GamePhaseAsset/MatchPhase")]
    public sealed class MatchPhase : GamePhaseAsset
    {
        public override async Task ExecutePhaseAsync(SimulationContext simulationContext)
        {
            PersistentWorldCameraPoints.Instance.SwapPoint("Onboard");

            var matchingPairs = simulationContext.CurrentState.GetMatchingPairs();
            GameState currentState = simulationContext.CurrentState;

            List<Task> matchSimulationWaitTasks = new List<Task>();
            List<GainWinPointsEvent> gainWinPointEvents = new List<GainWinPointsEvent>();
            
            foreach (var (playerAState, playerBState) in matchingPairs)
            {
                MatchContextEvent matchContextEvent = MatchContextEvent.RunMatch(currentState, playerAState, playerBState);
                matchContextEvent.Publish();
                simulationContext.CollectedEvents.AddEvent(matchContextEvent);
                
                currentState.MatchResults.AddMatchResult(matchContextEvent.Result);

                WinPointOnRound winPointOnRound = new WinPointOnRound(matchContextEvent.Round);
                int roundWinPoints = winPointOnRound.GetWinPoint();

                IPlayer winner = matchContextEvent.Result.Winner;
                ScoreEntry winnerEntry = new ScoreEntry(roundWinPoints, ScoreReason.ScoreByMatchWin);
                currentState.ScoreBoard.RegisterScoreEntry(winner, winnerEntry);
                
                matchSimulationWaitTasks.Add(playerAState.Player.WaitUntilConfirmToProceed(typeof(MatchContextEvent)));
                matchSimulationWaitTasks.Add(playerBState.Player.WaitUntilConfirmToProceed(typeof(MatchContextEvent)));

                int totalPoints = currentState.ScoreBoard.GetTotalWinPoints(winner);
                var gainWInPointEvent = new GainWinPointsEvent(winner, roundWinPoints, totalPoints, ScoreReason.ScoreByMatchWin);
                gainWinPointEvents.Add(gainWInPointEvent);
            }

            await Task.WhenAll(matchSimulationWaitTasks);

            foreach (var gainWinPointEvent in gainWinPointEvents)
            {
                gainWinPointEvent.Publish();
                simulationContext.CollectedEvents.AddEvent(gainWinPointEvent);
            }

            var waitConfirmTasks = gainWinPointEvents
                .Select(contextEvent => contextEvent.Player.WaitUntilConfirmToProceed(typeof(GainWinPointsEvent)));
            
            await Task.WhenAll(waitConfirmTasks);
        }
    }
}
