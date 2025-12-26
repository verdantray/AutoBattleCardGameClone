using System.Threading;
using System.Threading.Tasks;
using ProjectABC.Core;
using ProjectABC.Engine.UI;

namespace ProjectABC.Engine
{
    public class SwitchPositionProcessor : MatchEventProcessor<SwitchPositionEvent>
    {
        public override async Task ProcessEventAsync(SwitchPositionEvent matchEvent, CancellationToken token)
        {
            // TODO : refresh onboard simulation according to match snapshot
            
            MatchPosition startingPosition = ReferenceEquals(matchEvent.Attacker, Simulator.Model.player)
                ? MatchPosition.Attacking
                : MatchPosition.Defending;
            
            await MatchPositionAnnounceUI.ShowMatchPositionAsync(startingPosition, token);
        }
    }
}
