using System.Threading.Tasks;
using ProjectABC.Core;

namespace ProjectABC.Engine
{
    public interface IConfirmHandler
    {
        public void StartListening();
        public void StopListening();
        
        public Task WaitUntilConfirmAsync();
    }

    public interface IConfirmHandler<in TEvent> : IConfirmHandler, IContextEventListener<TEvent>
        where TEvent : class, IContextEvent
    {
    }
}
