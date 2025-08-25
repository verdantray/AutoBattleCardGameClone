using System.Collections.Generic;
using ProjectABC.Data;

namespace ProjectABC.Core
{
    public class DrawCardsFromPilesAction : IPlayerAction
    {
        public IPlayer Player { get; private set; }

        public readonly GradeType SelectedGrade;
        public readonly List<Card> DrawnCards;

        public DrawCardsFromPilesAction(IPlayer player, GradeType selectedGrade, List<Card> drawnCards)
        {
            Player = player;
            SelectedGrade = selectedGrade;
            DrawnCards = drawnCards;
        }
        
        public void ApplyState(GameState state)
        {
            PlayerState playerState = state.GetPlayerState(Player);
            playerState.Deck.AddRange(DrawnCards);
        }
    }
}