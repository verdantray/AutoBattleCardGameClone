using System.Threading;
using System.Threading.Tasks;
using ProjectABC.Core;
using ProjectABC.Engine.UI;

namespace ProjectABC.Engine
{
    public class SwitchPositionProcessor : MatchEventProcessor<SwitchPositionEvent>
    {
        private readonly ScaledTime _overlapDuration = 0.25f;
        private readonly ScaledTime _alignDuration = 0.25f;
        
        public override async Task ProcessEventAsync(SwitchPositionEvent matchEvent, CancellationToken token)
        {
            var matchPlayersUI = UIManager.Instance.OpenUI<MatchPlayersUI>();
            matchPlayersUI.ShowMatchPlayers(matchEvent.MatchSnapshot.MatchSideSnapShots.Values);
            
            
            var onboardController = Simulator.Model.onboardController;
            onboardController.SwitchPosition(matchEvent.Attacker, matchEvent.Defender);
            // TODO : refresh onboard simulation according to match snapshot
            // onboardController.RefreshOnboard();
            await onboardController.SetOverlapFieldCardsAsync(matchEvent.Defender, _overlapDuration, _alignDuration, token);
            
            MatchPosition startingPosition = matchEvent.Attacker.IsLocalPlayer
                ? MatchPosition.Attacking
                : MatchPosition.Defending;
            
            await MatchPositionAnnounceUI.ShowMatchPositionAsync(startingPosition, token);
        }
    }
}
