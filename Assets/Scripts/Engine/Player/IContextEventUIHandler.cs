using System;
using System.Threading.Tasks;
using ProjectABC.Core;

namespace ProjectABC.Engine
{
    public interface IContextEventUIHandler : IDisposable
    {
        public bool IsWaitConfirm { get; }

        public Task WaitUntilConfirmAsync();
    }

    public interface IContextEventUIHandler<in TEvent> : IContextEventUIHandler, IContextEventListener<TEvent>
        where TEvent : class, IContextEvent
    {
        
    }
}
