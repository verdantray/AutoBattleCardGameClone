using System.Collections.Generic;
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
            List<ScoreEntry> scoreEntries = new List<ScoreEntry>();
            
            foreach (var (playerAState, playerBState) in matchingPairs)
            {
                Debug.Log($"{playerAState.Player.Name} vs {playerBState.Player.Name}");
                
                MatchContextEvent matchContextEvent = MatchContextEvent.RunMatch(currentState, playerAState, playerBState);
                matchContextEvent.Publish();
                simulationContext.CollectedEvents.AddEvent(matchContextEvent);
                
                currentState.MatchResults.AddMatchResult(matchContextEvent.Result);

                WinPointOnRound winPointOnRound = new WinPointOnRound(matchContextEvent.Round);
                int roundWinPoints = winPointOnRound.GetWinPoint();

                ScoreEntry winnerEntry = new ScoreEntry(roundWinPoints, ScoreEntry.ScoreReason.ScoreByMatchWin);
                currentState.ScoreBoard.RegisterScoreEntry(matchContextEvent.Result.Winner, winnerEntry);
                
                matchSimulationWaitTasks.Add(playerAState.Player.WaitUntilConfirmToProceed(typeof(MatchContextEvent)));
                matchSimulationWaitTasks.Add(playerBState.Player.WaitUntilConfirmToProceed(typeof(MatchContextEvent)));
                
                scoreEntries.Add(winnerEntry);
            }

            await Task.WhenAll(matchSimulationWaitTasks);

            foreach (var scoreEntry in scoreEntries)
            {
                // publish context event
            }
            
            // open white board and show all players info
        }
    }
}
