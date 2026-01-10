using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectABC.Core;
using ProjectABC.Data;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectABC.Engine.UI
{
    public sealed class DeckConfigureCardsPage : MonoBehaviour, IDeckConstructionResultPage
    {
        [SerializeField] private Toggle[] cardListToggles;
        [SerializeField] private CardUIGridViewer cardGridViewer;

        private CardDataProviderByOption _provider = null;

        public bool IsOpen => gameObject.activeInHierarchy;
        public bool Initialized => cardGridViewer.Initialized;
        
        public Task GetInitializingTask() => cardGridViewer.GetInitializingTask();
        
        public void SetActive(bool active)
        {
            gameObject.SetActive(active);

            if (active)
            {
                ShowCardList();
            }
        }
        
        public void Refresh()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            
            ShowCardList();
        }

        private void Awake()
        {
            AddListeners();
        }

        private void OnDestroy()
        {
            RemoveListeners();
        }

        private void AddListeners()
        {
            foreach (var cardListToggle in cardListToggles)
            {
                cardListToggle.onValueChanged.AddListener(OnListToggle);
            }
        }

        private void RemoveListeners()
        {
            foreach (var cardListToggle in cardListToggles)
            {
                cardListToggle.onValueChanged.RemoveAllListeners();
            }
        }

        private void OnListToggle(bool value)
        {
            if (!value)
            {
                return;
            }
            
            ShowCardList();
        }

        public void SetSelectedClubs(ClubType selectedClubFlag)
        {
            _provider = new CardDataProviderByOption(selectedClubFlag);
        }

        private void ShowCardList()
        {
            int index = Array.FindIndex(cardListToggles, toggle => toggle.isOn);
            cardGridViewer.FetchData(_provider.GetCardData((CardListOption)index));
        }

        #region class for providing item data

        private enum CardListOption
        {
            StartingMembers = 0,
            RecruitableMembers = 1,
        }
        
        private class CardDataProviderByOption
        {
            private readonly List<CardData> _startingMembers;
            private readonly List<CardData> _recruitableMembers;
            
            public CardDataProviderByOption(ClubType selectedClubFlag)
            {
                _startingMembers = Storage.Instance.CardDataForStarting
                    .Where(data => selectedClubFlag.HasFlag(data.clubType))
                    .SelectMany(SelectAsAmount)
                    .ToList();

                _recruitableMembers = Storage.Instance.CardDataForPiles
                    .Where(data => selectedClubFlag.HasFlag(data.clubType))
                    .SelectMany(SelectAsAmount)
                    .ToList();
            }

            public IEnumerable<CardData> GetCardData(CardListOption option)
            {
                return option switch
                {
                    CardListOption.StartingMembers => _startingMembers,
                    CardListOption.RecruitableMembers => _recruitableMembers,
                    _ => throw new ArgumentOutOfRangeException(nameof(option), option, null)
                };
            }

            private static IEnumerable<CardData> SelectAsAmount(CardData cardData)
            {
                var cardDataList = new List<CardData>();
                for (int i = 0; i < cardData.amount; i++)
                {
                    cardDataList.Add(cardData);
                }
                
                return cardDataList;
            }
        }

        #endregion
    }
}
