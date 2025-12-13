using System.Threading;
using System.Threading.Tasks;
using ProjectABC.Core;
using ProjectABC.Engine.UI;

namespace ProjectABC.Engine
{
    public interface IMatchEventProcessor
    {
        public Task ProcessEventAsync(IMatchEvent matchEvent, CancellationToken token = default);
        public void SkipEvent(IMatchEvent matchEvent);
    }
    
    public abstract class MatchEventProcessor<T> : IMatchEventProcessor where T : IMatchEvent
    {
        public abstract Task ProcessEventAsync(T matchEvent, CancellationToken token = default);
        public abstract void SkipEvent(T matchEvent);

        public async Task ProcessEventAsync(IMatchEvent matchEvent, CancellationToken token = default)
        {
            await ProcessEventAsync((T)matchEvent, token);
        }

        public void SkipEvent(IMatchEvent matchEvent)
        {
            SkipEvent((T)matchEvent);
        }
    }

    public sealed class MatchStartProcessor : MatchEventProcessor<MatchStartEvent>
    {
        public override async Task ProcessEventAsync(MatchStartEvent matchEvent, CancellationToken token = default)
        {
            IPlayer[] players = { matchEvent.Attacker, matchEvent.Defender };

            await MatchPlayersAnnounceUI.ShowMatchPlayersAsync(players, token);

            UIManager.Instance.OpenUI<MatchMenuUI>();
            
            var matchUI = UIManager.Instance.OpenUI<MatchPlayersUI>();
            matchUI.ShowMatchPlayers(matchEvent.MatchMatchSnapshot.MatchSideSnapShots.Values);
        }

        public override void SkipEvent(MatchStartEvent matchEvent)
        {
            
        }
    }
}
