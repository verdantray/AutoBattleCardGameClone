using System.Threading.Tasks;
using ProjectABC.Data;

namespace ProjectABC.Core
{
    public interface IPlayer
    {
        public string Name { get; }
        public bool IsLocalPlayer { get; }

        // temporary implements, DeckConstructAsync will implement each inherits after add more club types
        public Task<IPlayerAction> DeckConstructAsync(ClubType fixedClubFlag, ClubType selectableClubFlag);
        public Task<IPlayerAction> RecruitCardsAsync(PlayerState myState, RecruitOnRound recruitOnRound);
        public Task<IPlayerAction> DeleteCardsAsync(PlayerState myState);
        public Task WaitUntilConfirmToProceed();
    }

    public interface IPlayerAction
    {
        public IPlayer Player { get; }
        public void ApplyState(GameState state);
        public void ApplyContextEvent(SimulationContextEvents events);
    }
}
