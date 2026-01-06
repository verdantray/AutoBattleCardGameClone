using System.Threading;
using System.Threading.Tasks;
using ProjectABC.Core;


namespace ProjectABC.Engine
{
    public class MatchFinishProcessor : MatchEventProcessor<MatchFinishEvent>
    {
        public override async Task ProcessEventAsync(MatchFinishEvent matchEvent, CancellationToken token)
        {
            
        }
    }
}
