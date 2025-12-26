using System.Threading;
using System.Threading.Tasks;
using ProjectABC.Core;
using UnityEngine;

namespace ProjectABC.Engine
{
    public sealed class SuccessAttackProcessor : MatchEventProcessor<SuccessAttackEvent>
    {
        public override Task ProcessEventAsync(SuccessAttackEvent matchEvent, CancellationToken token)
        {
            Debug.Log("공격자가 공격에 성공");
            return Task.CompletedTask;
        }
    }
}
