using System.Threading.Tasks;
using ProjectABC.Core;

namespace ProjectABC.Engine
{
    public interface IConfirmHandler
    {
        public Task WaitUntilConfirmAsync();
    }

    public interface IConfirmHandler<in TEvent> : IConfirmHandler, IContextEventListener<TEvent>
        where TEvent : class, IContextEvent
    {
        
    }
}
