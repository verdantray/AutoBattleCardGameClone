using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectABC.Data;

namespace ProjectABC.Core
{
    public class DeckConstructionPhase : IGamePhase
    {
        public async Task ExecutePhaseAsync(SimulationContext simulationContext)
        {
            GradeType defaultDeckType = Enum.Parse<GradeType>(GameConst.GameOption.DEFAULT_LEVEL_TYPE);
            GameState currentState = simulationContext.CurrentState;
            
            ClubType randomSelectedClubTypeFlag = GetRandomSelectedSetTypeFlag();
            CardData[] cardDataForPiles = Storage.Instance.CardData
                .Where(data => randomSelectedClubTypeFlag.HasFlag(data.clubType) && data.gradeType != defaultDeckType)
                .ToArray();

            List<Card> cardsForPutToFiles = new List<Card>();

            foreach (CardData cardData in cardDataForPiles)
            {
                for (int i = 0; i < cardData.amount; i++)
                {
                    Card card = new Card(cardData);
                    cardsForPutToFiles.Add(card);
                }
            }

            foreach (var levelGroup in cardsForPutToFiles.GroupBy(card => card.GradeType))
            {
                var cardPile = currentState.LevelCardPiles[levelGroup.Key];
                await cardPile.AddRangeAsync(levelGroup);
                await cardPile.ShuffleAsync();
            }

            var cardPilesConstructionEvent = new CardPilesConstructionConsoleEvent(randomSelectedClubTypeFlag, cardsForPutToFiles);
            simulationContext.CollectedEvents.Add(cardPilesConstructionEvent);

            CardData[] startingCardData = Storage.Instance.CardData
                .Where(data => data.gradeType == defaultDeckType)
                .ToArray();
            
            foreach (PlayerState playerState in currentState.PlayerStates)
            {
                foreach (CardData cardData in startingCardData)
                {
                    for (int i = 0; i < cardData.amount; i++)
                    {
                        Card card = new Card(cardData);
                        playerState.Deck.Add(card);
                    }
                }
                
                var deckConstructionEvent = new DeckConstructionConsoleEvent(playerState.Player, playerState.Deck);
                simulationContext.CollectedEvents.Add(deckConstructionEvent);
            }
        }

        private ClubType GetRandomSelectedSetTypeFlag()
        {
            ClubType mustIncludeClubType = Enum.Parse<ClubType>(GameConst.GameOption.DEFAULT_SET_TYPE, true);

            List<ClubType> allSetTypes = new List<ClubType>(Enum.GetValues(typeof(ClubType)).Cast<ClubType>());
            allSetTypes.Remove(mustIncludeClubType);

            System.Random random = new System.Random();
            ReadOnlySpan<ClubType> selectedSetTypes = allSetTypes
                .OrderBy(_ => random.Next())
                .Take(GameConst.GameOption.SELECT_SET_TYPES_AMOUNT - 1)
                .ToArray();

            ClubType selectedClubTypeFlag = mustIncludeClubType;

            foreach (ClubType selectedSetType in selectedSetTypes)
            {
                selectedClubTypeFlag |= selectedSetType;
            }

            return selectedClubTypeFlag;
        }
    }
}
