using System.Threading;
using System.Threading.Tasks;
using ProjectABC.Core;

namespace ProjectABC.Engine
{
    public sealed class DrawCardToFieldProcessor : MatchEventProcessor<DrawCardToFieldEvent>
    {
        private readonly ScaledTime _drawDuration = 0.5f;
        
        public override Task ProcessEventAsync(DrawCardToFieldEvent matchEvent, CancellationToken token = default)
        {
            return Task.CompletedTask;
        }
    }
}
