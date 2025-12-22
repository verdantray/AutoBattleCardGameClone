using System.Threading;
using System.Threading.Tasks;
using ProjectABC.Core;


namespace ProjectABC.Engine
{
    public interface IMatchEventProcessor
    {
        public Task ProcessEventAsync(IMatchEvent matchEvent, CancellationToken token = default);
    }
    
    public abstract class MatchEventProcessor<T> : IMatchEventProcessor where T : IMatchEvent
    {
        public abstract Task ProcessEventAsync(T matchEvent, CancellationToken token = default);

        public async Task ProcessEventAsync(IMatchEvent matchEvent, CancellationToken token = default)
        {
            await ProcessEventAsync((T)matchEvent, token);
        }
    }
}
