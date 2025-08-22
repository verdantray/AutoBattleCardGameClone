using System.Threading.Tasks;

namespace ProjectABC.Core
{
    public interface IPlayer
    {
        public string Name { get; }
        public Task<DrawCardsFromPilesAction> DrawCardsFromPilesAsync(int mulliganChances, RecruitOnRound recruitOnRound, LevelCardPiles levelCardPiles);
    }

    public interface IPlayerAction
    {
        public IPlayer Player { get; }
        public void ApplyState(GameState state);
    }
}
