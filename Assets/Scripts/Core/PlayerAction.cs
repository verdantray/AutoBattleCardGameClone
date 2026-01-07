using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectABC.Data;

namespace ProjectABC.Core
{
    public class DeckConstructAction : IPlayerAction
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
                .Where(ClubFilter)
                .SelectMany(CreateCardsFromData)
                .ToList();
            
            _cardDataForPiles = Storage.Instance.CardDataForPiles
                .Where(ClubFilter)
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

                foreach (var card in gradeGroup)
                {
                    playerState.GradeCardPiles[gradeType].Add(card);
                }
                
                playerState.GradeCardPiles[gradeType].Shuffle();
            }
        }

        public void ApplyContextEvent(SimulationContextEvents events)
        {
            DeckConstructionEvent consoleEvent = new DeckConstructionEvent(Player, _selectedClubsFlag);
            
            consoleEvent.Publish();
            events.AddEvent(consoleEvent);
        }

        public Task GetWaitConfirmTask()
        {
            return Player.WaitUntilConfirmToProceed(typeof(DeckConstructionEvent));
        }

        private bool ClubFilter(CardData cardData)
        {
            return _selectedClubsFlag.HasFlag(cardData.clubType);
        }

        private IEnumerable<Card> CreateCardsFromData(CardData cardData)
        {
            List<Card> cards = new List<Card>();
            
            for (int i = 0; i < cardData.amount; i++)
            {
                cards.Add(new Card(Player, cardData));
            }

            return cards;
        }
    }
    
    public class RecruitCardsAction : IPlayerAction
    {
        public IPlayer Player { get; private set; }

        private readonly GradeType _selectedGrade;
        private readonly List<Card> _drawnCards;
        private readonly List<IContextEvent> _recruitContextEvents = new List<IContextEvent>();

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

            foreach (var card in _drawnCards)
            {
                if (!card.CardEffect.TryApplyEffectOnRecruit(Player, state, out var recruitEvent))
                {
                    continue;
                }
                
                _recruitContextEvents.Add(recruitEvent);
            }
        }

        public void ApplyContextEvent(SimulationContextEvents events)
        {
            var contextEvent = new RecruitCardsEvent(Player, _selectedGrade, _drawnCards);
            contextEvent.Publish();
            
            events.AddEvent(contextEvent);

            if (_recruitContextEvents.Count == 0)
            {
                return;
            }
            
            foreach (var recruitEvent in _recruitContextEvents)
            {
                recruitEvent.Publish();
                events.AddEvent(recruitEvent);
            }
        }

        public Task GetWaitConfirmTask()
        {
            HashSet<Type> eventTypes = new HashSet<Type> { typeof(RecruitCardsEvent) };
            foreach (var recruitEvent in _recruitContextEvents)
            {
                eventTypes.Add(recruitEvent.GetType());
            }

            return Task.WhenAll(eventTypes.Select(eventType => Player.WaitUntilConfirmToProceed(eventType)));
        }
    }
    
    public class DeleteCardsAction : IPlayerAction
    {
        public IPlayer Player { get; private set; }

        private readonly List<Card> _deleteCards;
        
        public DeleteCardsAction(IPlayer player, List<Card> deleteCards)
        {
            Player = player;
            _deleteCards = deleteCards;
        }
        
        public void ApplyState(GameState state)
        {
            PlayerState playerState = state.GetPlayerState(Player);

            foreach (var card in _deleteCards)
            {
                playerState.Deck.Remove(card);
            }
            
            playerState.Deleted.AddRange(_deleteCards);
        }

        public void ApplyContextEvent(SimulationContextEvents events)
        {
            DeleteCardsConsoleEvent consoleEvent = new DeleteCardsConsoleEvent(Player, _deleteCards);
            
            consoleEvent.Publish();
            events.AddEvent(consoleEvent);
        }

        public Task GetWaitConfirmTask()
        {
            return Task.CompletedTask;
        }
    }
}