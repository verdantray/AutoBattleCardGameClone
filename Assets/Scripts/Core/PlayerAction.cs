using System.Collections.Generic;
using ProjectABC.Data;

namespace ProjectABC.Core
{
    public class DrawCardsFromPilesAction : IPlayerAction
    {
        public IPlayer Player { get; private set; }

        public readonly LevelType SelectedLevel;
        public readonly List<Card> DrawnCards;

        public DrawCardsFromPilesAction(IPlayer player, LevelType selectedLevel, List<Card> drawnCards)
        {
            Player = player;
            SelectedLevel = selectedLevel;
            DrawnCards = drawnCards;
        }
        
        public void ApplyState(GameState state)
        {
            PlayerState playerState = state.GetPlayerState(Player);
            playerState.Deck.AddRange(DrawnCards);
        }
    }
}