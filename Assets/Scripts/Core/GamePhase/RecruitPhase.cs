using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectABC.Data;

namespace ProjectABC.Core
{
	public class RecruitPhase : IGamePhase
    {
        public async Task ExecutePhaseAsync(SimulationContext simulationContext)
        {
            GameState currentState = simulationContext.CurrentState;
            RecruitOnRound recruitOnRound = new RecruitOnRound(currentState.Round);
                
            List<PlayerState> allPlayerStates = currentState.PlayerStates;
            List<Task<RecruitCardsAction>> tasks = new List<Task<RecruitCardsAction>>();

            foreach (PlayerState playerState in allPlayerStates)
            {
                IPlayer player = playerState.Player;
                var playerActionTask = player.RecruitCardsAsync(playerState, recruitOnRound);
                    
                tasks.Add(playerActionTask);
            }
                
            await Task.WhenAll(tasks);

            foreach (RecruitCardsAction action in tasks.Select(task => task.Result))
            {
                action.ApplyState(currentState);

                RecruitConsoleEvent contextEvent = action.GetContextEvent();
                contextEvent.Publish();
                
                simulationContext.CollectedEvents.Add(contextEvent);
            }
        }
    }

    public class RecruitOnRound
    {
        private readonly List<Tuple<GradeType, int>> _recruitLevelAndAmounts;
        
        public RecruitOnRound(int round)
        {
            _recruitLevelAndAmounts = Storage.Instance.RecruitData
                .Where(data => data.round == round)
                .Select(ElementSelector)
                .ToList();
        }

        private static Tuple<GradeType, int> ElementSelector(RecruitData recruitData)
        {
            return new Tuple<GradeType, int>(recruitData.recruitGradeType, recruitData.amount);
        }

        public IReadOnlyList<Tuple<GradeType, int>> GetRecruitLevelAmountPairs()
        {
            return _recruitLevelAndAmounts;
        }
    }
}