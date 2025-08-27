using System.Collections.Generic;
using System.Linq;
using ProjectABC.Data;

namespace ProjectABC.Core
{
    public class DeckConstructAction : IPlayerAction<DeckConstructionConsoleEvent>
    {
        public IPlayer Player { get; private set; }
        
        private readonly ClubType _selectedClubsFlag;
        private readonly List<Card> _cardDataForStarting;
        private readonly List<Card> _cardDataForPiles;

        public DeckConstructAction(IPlayer player, ClubType selectedClubsFlag)
        {
            Player = player;
            _selectedClubsFlag = selectedClubsFlag;

            _cardDataForStarting = Storage.Instance.CardDataForStarting
                .SelectMany(CreateCardsFromData)
                .ToList();
            
            _cardDataForPiles = Storage.Instance.CardDataForPiles
                .Where(cardData => _selectedClubsFlag.HasFlag(cardData.clubType))
                .SelectMany(CreateCardsFromData)
                .ToList();
        }
        
        public void ApplyState(GameState state)
        {
            PlayerState playerState = state.GetPlayerState(Player);
            
            playerState.Deck.AddRange(_cardDataForStarting);
            
            foreach (var gradeGroup in _cardDataForPiles.GroupBy(card => card.GradeType))
            {
                GradeType gradeType = gradeGroup.Key;
                
                playerState.GradeCardPiles[gradeType].AddRange(gradeGroup);
                playerState.GradeCardPiles[gradeType].Shuffle();
            }
        }

        private static IEnumerable<Card> CreateCardsFromData(CardData cardData)
        {
            List<Card> cards = new List<Card>();
            
            for (int i = 0; i < cardData.amount; i++)
            {
                cards.Add(new Card(cardData));
            }

            return cards;
        }
        
        public DeckConstructionConsoleEvent GetContextEvent()
        {
            return new DeckConstructionConsoleEvent(
                Player,
                _selectedClubsFlag,
                _cardDataForPiles,
                _cardDataForStarting
            );
        }
    }
    
    public class RecruitCardsAction : IPlayerAction<RecruitConsoleEvent>
    {
        public IPlayer Player { get; private set; }

        private readonly GradeType _selectedGrade;
        private readonly List<Card> _drawnCards;

        public RecruitCardsAction(IPlayer player, GradeType selectedGrade, List<Card> drawnCards)
        {
            Player = player;
            _selectedGrade = selectedGrade;
            _drawnCards = drawnCards;
        }
        
        public void ApplyState(GameState state)
        {
            PlayerState playerState = state.GetPlayerState(Player);
            playerState.Deck.AddRange(_drawnCards);
        }
        
        public RecruitConsoleEvent GetContextEvent()
        {
            return new RecruitConsoleEvent(Player, _selectedGrade, _drawnCards);
        }
    }
}