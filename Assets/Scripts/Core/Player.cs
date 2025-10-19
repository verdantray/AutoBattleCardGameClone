using System.Threading.Tasks;
using ProjectABC.Data;

namespace ProjectABC.Core
{
    public interface IPlayer
    {
        public string Name { get; }

        // temporary implements, DeckConstructAsync will implement each inherits after add more club types
        public Task<IPlayerAction<IContextEvent>> DeckConstructAsync();
        public Task<IPlayerAction<IContextEvent>> RecruitCardsAsync(PlayerState myState, RecruitOnRound recruitOnRound);
        public Task<IPlayerAction<IContextEvent>> DeleteCardsAsync(PlayerState myState);
        public Task WaitUntilConfirmToProceed();
    }

    public interface IPlayerAction
    {
        public IPlayer Player { get; }
        public void ApplyState(GameState state);
    }

    public interface IPlayerAction<out T> : IPlayerAction where T : class, IContextEvent
    {
        public T GetContextEvent();
        
        public void ApplyContextEvent(SimulationContextEvents events)
        {
            var contextEvent = GetContextEvent();
            contextEvent.Publish();
            
            events.AddEvent(contextEvent);
        }
    }
}
