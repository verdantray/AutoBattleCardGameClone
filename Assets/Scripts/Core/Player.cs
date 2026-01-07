using System;
using System.Threading.Tasks;

namespace ProjectABC.Core
{
    public interface IPlayerEntry
    {
        public IPlayer GetPlayer();
    }
    
    public interface IPlayer
    {
        public string Name { get; }
        public bool IsLocalPlayer { get; }
        
        public Task<IPlayerAction> DeckConstructAsync();
        public Task<IPlayerAction> RecruitCardsAsync(PlayerState myState, RecruitOnRound recruitOnRound);
        public Task<IPlayerAction> DeleteCardsAsync(PlayerState myState);
        public Task WaitUntilConfirmToProceed(Type eventType);
    }

    public interface IPlayerAction
    {
        public IPlayer Player { get; }
        public void ApplyState(GameState state);
        public void ApplyContextEvent(SimulationContextEvents events);
        public Task GetWaitConfirmTask();
    }
}
