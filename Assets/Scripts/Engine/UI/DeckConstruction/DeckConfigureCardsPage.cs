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

        private CardSnapshotProviderByOption _provider = null;

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
            _provider = new CardSnapshotProviderByOption(selectedClubFlag);
        }

        private void ShowCardList()
        {
            int index = Array.FindIndex(cardListToggles, toggle => toggle.isOn);
            cardGridViewer.FetchData(_provider.GetCardSnapshots((CardListOption)index));
        }

        #region class for providing item data

        private enum CardListOption
        {
            StartingMembers = 0,
            RecruitableMembers = 1,
        }
        
        private class CardSnapshotProviderByOption
        {
            private readonly List<CardReference> _startingMembers = new List<CardReference>();
            private readonly List<CardReference> _recruitableMembers = new List<CardReference>();
            
            public CardSnapshotProviderByOption(ClubType selectedClubFlag)
            {
                var startingMembers = Storage.Instance.CardDataForStarting
                    .Where(data => selectedClubFlag.HasFlag(data.clubType))
                    .SelectMany(GetCardSnapshotsFromData);

                var recruitableMembers = Storage.Instance.CardDataForPiles
                    .Where(data => selectedClubFlag.HasFlag(data.clubType))
                    .SelectMany(GetCardSnapshotsFromData);
                
                _startingMembers.AddRange(startingMembers);
                _recruitableMembers.AddRange(recruitableMembers);
            }

            public IReadOnlyCollection<CardReference> GetCardSnapshots(CardListOption option)
            {
                return option switch
                {
                    CardListOption.StartingMembers => _startingMembers,
                    CardListOption.RecruitableMembers => _recruitableMembers,
                    _ => throw new ArgumentOutOfRangeException(nameof(option), option, null)
                };
            }

            private static IEnumerable<CardReference> GetCardSnapshotsFromData(CardData data)
            {
                List<CardReference> result = new List<CardReference>();

                for (int i = 0; i < data.amount; i++)
                {
                    result.Add(new CardReference(data.id));
                }

                return result;
            }
        }

        #endregion
    }
}
