using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectABC.Data;
using UnityEngine;

namespace ProjectABC.Core
{
	public class RecruitPhase : IGamePhase
    {
        public async Task ExecutePhaseAsync(SimulationContext simulationContext)
        {
            try
            {
                GameState currentState = simulationContext.CurrentState;
                RecruitOnRound recruitOnRound = new RecruitOnRound(currentState.Round);
                
                List<PlayerState> allPlayerStates = currentState.PlayerStates;
                List<Task<DrawCardsFromPilesAction>> tasks = new List<Task<DrawCardsFromPilesAction>>();

                foreach (PlayerState playerState in allPlayerStates)
                {
                    IPlayer player = playerState.Player;
                    var playerActionTask = player.DrawCardsFromPilesAsync(
                        playerState.MulliganChances,
                        recruitOnRound,
                        currentState.LevelCardPiles
                    );
                    
                    tasks.Add(playerActionTask);
                }
                
                await Task.WhenAll(tasks);

                foreach (DrawCardsFromPilesAction action in tasks.Select(task => task.Result))
                {
                    action.ApplyState(currentState);
                    
                    RecruitConsoleEvent contextEvent = new RecruitConsoleEvent(action.Player, action.SelectedLevel, action.DrawnCards);
                    simulationContext.CollectedEvents.Add(contextEvent);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{nameof(RecruitPhase)} exception : {e}");
                throw;
            }
        }
    }

    public class RecruitOnRound
    {
        private readonly List<Tuple<LevelType, int>> recruitLevelAndAmounts;
        
        public RecruitOnRound(int round)
        {
            recruitLevelAndAmounts = Storage.Instance.RecruitData
                .Where(data => data.round == round)
                .Select(ElementSelector)
                .ToList();
        }

        private static Tuple<LevelType, int> ElementSelector(RecruitData recruitData)
        {
            return new Tuple<LevelType, int>(recruitData.recruitLevelType, recruitData.amount);
        }

        public IReadOnlyList<Tuple<LevelType, int>> GetRecruitLevelAmountPairs()
        {
            return recruitLevelAndAmounts;
        }
    }
}