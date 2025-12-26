using System.Collections.Generic;
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

            MatchPosition startingPosition = ReferenceEquals(matchEvent.Attacker, Simulator.Model.player)
                ? MatchPosition.Attacking
                : MatchPosition.Defending;
            
            await MatchPositionAnnounceUI.ShowMatchPositionAsync(startingPosition, token);
            
            // onboard animations
            var onboardController = Simulator.Model.onboardController;
            List<Task> deckTasks = new List<Task>();
            
            foreach (var (player, matchSideSnapshot) in matchEvent.MatchMatchSnapshot.MatchSideSnapShots)
            {
                var task = onboardController.SetCardsToDeckPileAsync(
                    player,
                    matchSideSnapshot.Deck,
                    _deckSetDelay,
                    _deckSetDuration,
                    token
                );
                
                deckTasks.Add(task);
            }
            
            await Task.WhenAll(deckTasks);
        }
    }
}
