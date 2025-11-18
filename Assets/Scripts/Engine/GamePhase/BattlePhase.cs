using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectABC.Core;
using UnityEngine;

namespace ProjectABC.Engine
{
    [CreateAssetMenu(fileName = nameof(BattlePhase), menuName = "Scripting/ScriptableObject Script Menu/GamePhaseAsset/BattlePhase")]
    public sealed class BattlePhase : GamePhaseAsset
    {
        public override async Task ExecutePhaseAsync(SimulationContext simulationContext)
        {
            PersistentWorldCameraPoints.Instance.SwapPoint("Onboard");

            var matchingPairs = simulationContext.CurrentState.GetMatchingPairs();
            GameState currentState = simulationContext.CurrentState;

            List<ScoreEntry> scoreEntries = new List<ScoreEntry>();
            
            foreach (var (playerAState, playerBState) in matchingPairs)
            {
                MatchContextEvent matchContextEvent = MatchContextEvent.RunMatch(currentState, playerAState, playerBState);
                matchContextEvent.Publish();
                simulationContext.CollectedEvents.AddEvent(matchContextEvent);
                
                currentState.MatchResults.AddMatchResult(matchContextEvent.Result);

                WinPointOnRound winPointOnRound = new WinPointOnRound(matchContextEvent.Round);
                int roundWinPoints = winPointOnRound.GetWinPoint();

                ScoreEntry winnerEntry = new ScoreEntry(roundWinPoints, ScoreEntry.ScoreReason.ScoreByMatchWin);
                currentState.ScoreBoard.RegisterScoreEntry(matchContextEvent.Result.Winner, winnerEntry);
                
                scoreEntries.Add(winnerEntry);
            }

            await Task.WhenAll(simulationContext.GetTasksOfAllPlayersConfirmToProceed());

            foreach (var scoreEntry in scoreEntries)
            {
                // publish context event
            }
            
            // open white board and show all players info
        }
    }
}
