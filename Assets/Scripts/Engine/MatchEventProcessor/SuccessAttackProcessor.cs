using System.Threading;
using System.Threading.Tasks;
using ProjectABC.Core;
using ProjectABC.Engine.UI;

namespace ProjectABC.Engine
{
    public sealed class SuccessAttackProcessor : MatchEventProcessor<SuccessAttackEvent>
    {
        public override Task ProcessEventAsync(SuccessAttackEvent matchEvent, CancellationToken token)
        {
            MatchLogUI.SendLog($"{matchEvent.Attacker.Name}이(가) 공격에 성공");
            
            return Task.CompletedTask;
        }
    }
}
