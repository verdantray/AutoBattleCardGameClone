using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectABC.Data;

namespace ProjectABC.Core
{
    public class DeckConstructAction : IPlayerAction
    {
        public IPlayer Player { get; }
        
        private readonly ClubType _selectedClubsFlag;
        private readonly List<CardData> _cardDataForStarting;
        private readonly List<CardData> _cardDataForPiles;

        public DeckConstructAction(IPlayer player, ClubType selectedClubsFlag)
        {
            Player = player;
            _selectedClubsFlag = selectedClubsFlag;

            _cardDataForStarting = Storage.Instance.CardDataForStarting
                .Where(ClubFilter)
                .ToList();
            
            _cardDataForPiles = Storage.Instance.CardDataForPiles
                .Where(ClubFilter)
                .ToList();
        }
        
        public void ApplyState(GameState state)
        {
            PlayerState playerState = state.GetPlayerState(Player);
            
            playerState.RecruitedCardIds.Initialize(_cardDataForStarting);
            playerState.GradeCardPiles.Initialize(_cardDataForPiles);
            
            playerState.RecruitedCardIds.Shuffle();
            
            foreach (var idQueueForGrade in playerState.GradeCardPiles.Values)
            {
                // TODO : use PCG32
                idQueueForGrade.Shuffle();
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
            return Task.CompletedTask;
        }

        private bool ClubFilter(CardData cardData)
        {
            return _selectedClubsFlag.HasFlag(cardData.clubType);
        }
    }
    
    public class RecruitCardsAction : IPlayerAction
    {
        public IPlayer Player { get; }

        private readonly GradeType _selectedGrade;
        private readonly List<string> _drawnCardIds;
        private readonly List<IContextEvent> _recruitContextEvents = new List<IContextEvent>();

        public RecruitCardsAction(IPlayer player, GradeType selectedGrade, List<string> drawnCardIds)
        {
            Player = player;
            _selectedGrade = selectedGrade;
            _drawnCardIds = drawnCardIds;
        }
        
        public void ApplyState(GameState state)
        {
            PlayerState playerState = state.GetPlayerState(Player);
            playerState.RecruitedCardIds.EnqueueCardIds(_drawnCardIds);

            foreach (var cardId in _drawnCardIds)
            {
                var cardData = Storage.Instance.GetCardData(cardId);
                var card = new Card(Player, cardData);
                
                if (!card.CardEffect.TryApplyEffectOnRecruit(Player, state, out var recruitEvent))
                {
                    continue;
                }
                
                _recruitContextEvents.Add(recruitEvent);
            }
        }

        public void ApplyContextEvent(SimulationContextEvents events)
        {
            var contextEvent = new RecruitCardsEvent(Player, _selectedGrade, _drawnCardIds);
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
        public IPlayer Player { get; }

        private readonly List<string> _deleteCardIds;
        
        public DeleteCardsAction(IPlayer player, List<string> deleteCardIds)
        {
            Player = player;
            _deleteCardIds = deleteCardIds;
        }
        
        public void ApplyState(GameState state)
        {
            PlayerState playerState = state.GetPlayerState(Player);
            playerState.DismissedCardIds.EnqueueCardIds(_deleteCardIds);
        }

        public void ApplyContextEvent(SimulationContextEvents events)
        {
            DeleteCardsEvent consoleEvent = new DeleteCardsEvent(Player, _deleteCardIds);
            
            consoleEvent.Publish();
            events.AddEvent(consoleEvent);
        }

        public Task GetWaitConfirmTask()
        {
            return Player.WaitUntilConfirmToProceed(typeof(DeleteCardsEvent));
        }
    }
}