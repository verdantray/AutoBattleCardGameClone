using System;
using System.Threading.Tasks;
using ProjectABC.Data;

namespace ProjectABC.Core
{
    public interface IPlayer
    {
        public string Name { get; }

        // temporary implements, DeckConstructAsync will implement each inherits after add more club types
        public Task<DeckConstructAction> DeckConstructAsync()
        {
            ClubType[] gradeTypes =
            {
                ClubType.Council,
                ClubType.Coastline,
                ClubType.Band,
                ClubType.GameDevelopment,
                ClubType.HauteCuisine,
                ClubType.Unregistered,
                ClubType.TraditionExperience
            };
            
            ClubType flag = 0;

            foreach (var gradeType in gradeTypes)
            {
                flag |= gradeType;
            }

            DeckConstructAction action = new DeckConstructAction(this, flag);
            Task<DeckConstructAction> task = Task.FromResult(action);

            return task;
        }
        
        public Task<RecruitCardsAction> RecruitCardsAsync(PlayerState myState, RecruitOnRound recruitOnRound);
        public Task<DeleteCardsAction> DeleteCardsAsync(PlayerState myState);
        public Task WaitUntilConfirmToProceed();
    }

    public interface IPlayerAction
    {
        public IPlayer Player { get; }
        public void ApplyState(GameState state);
    }

    public interface IPlayerAction<out T> : IPlayerAction where T : IContextEvent
    {
        public T GetContextEvent();
    }
}
