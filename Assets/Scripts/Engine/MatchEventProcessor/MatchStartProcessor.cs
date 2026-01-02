using System.Threading;
using System.Threading.Tasks;
using ProjectABC.Core;
using ProjectABC.Engine.UI;


namespace ProjectABC.Engine
{
    public sealed class MatchStartProcessor : MatchEventProcessor<MatchStartEvent>
    {
        private readonly ScaledTime _deckSetDelay = 0.5f;
        private readonly ScaledTime _deckSetDuration = 1.0f;
        
        public override async Task ProcessEventAsync(MatchStartEvent matchEvent, CancellationToken token)
        {
            IPlayer[] players = { matchEvent.Attacker, matchEvent.Defender };

            await MatchPlayersAnnounceUI.ShowMatchPlayersAsync(players, token);

            UIManager.Instance.OpenUI<MatchMenuUI>();
            
            var matchUI = UIManager.Instance.OpenUI<MatchPlayersUI>();
            matchUI.ShowMatchPlayers(matchEvent.MatchMatchSnapshot.MatchSideSnapShots.Values);

            MatchPosition startingPosition = matchEvent.Attacker.IsLocalPlayer
                ? MatchPosition.Attacking
                : MatchPosition.Defending;
            
            await MatchPositionAnnounceUI.ShowMatchPositionAsync(startingPosition, token);
            var onboardController = Simulator.Model.onboardController;
            
            foreach (var (player, matchSideSnapshot) in matchEvent.MatchMatchSnapshot.MatchSideSnapShots)
            {
                onboardController.RegisterOnboard(player, matchSideSnapshot.Position, matchSideSnapshot.Deck);
            }

            await onboardController.InitializeOnboardAsync(_deckSetDelay, _deckSetDuration, token);
        }
    }
}
