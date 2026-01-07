using System.Threading;
using System.Threading.Tasks;
using ProjectABC.Core;
using ProjectABC.Engine.UI;


namespace ProjectABC.Engine
{
    public class MatchFinishProcessor : MatchEventProcessor<MatchFinishEvent>
    {
        public override Task ProcessEventAsync(MatchFinishEvent matchEvent, CancellationToken token)
        {
            Simulator.Model.onboardController.ClearOnboard();

            UIManager.Instance.CloseUI<MatchPlayersUI>();
            UIManager.Instance.CloseUI<MatchMenuUI>();
            
            MatchResultUI.ShowMatchResult(matchEvent);
            
            return Task.CompletedTask;
        }
    }
}
