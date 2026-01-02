using System.Threading;
using System.Threading.Tasks;
using ProjectABC.Core;
using ProjectABC.Engine.UI;

namespace ProjectABC.Engine
{
    public class GainWinPointsByEffectProcessor : MatchEventProcessor<GainWinPointsByCardEffectEvent>
    {
        public override Task ProcessEventAsync(GainWinPointsByCardEffectEvent matchEvent, CancellationToken token)
        {
            var matchPlayersUI = UIManager.Instance.OpenUI<MatchPlayersUI>();

            return matchPlayersUI.GetShowChangingScoreTask(matchEvent.Player, matchEvent.GainPoints, matchEvent.TotalPoints, token);
        }
    }
}
