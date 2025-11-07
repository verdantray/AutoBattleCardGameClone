using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectABC.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectABC.Engine.UI
{
    public interface IDeckConstructionResultPage
    {
        public bool IsOpen { get; }
        public bool Initialized { get; }
        public Task GetInitializingTask();
        public void SetActive(bool active);
        public void Refresh();
    }
    
    public sealed class DeckConstructionResultUI : UIElement
    {
        [SerializeField] private Button btnPrev;
        [SerializeField] private Button btnNext;
        [SerializeField] private PlayerSelectedClubListViewer playerSelectedClubListViewer;
        [SerializeField] private DeckConfigureCardsPage deckConfigureCardsPage;

        private readonly IDeckConstructionResultPage[] _pages = new IDeckConstructionResultPage[2];
        private int _pageIndex = 0;

        private void Awake()
        {
            _pages[0] = playerSelectedClubListViewer;
            _pages[1] = deckConfigureCardsPage;
            
            btnPrev.onClick.AddListener(OnClickPrev);
            btnNext.onClick.AddListener(OnClickNext);
        }

        private void OnDestroy()
        {
            btnPrev.onClick.RemoveAllListeners();
            btnNext.onClick.RemoveAllListeners();
        }

        private void OnClickPrev()
        {
            int prevIndex = Mathf.Clamp(_pageIndex - 1, 0, _pages.Length - 1);
            if (_pageIndex == prevIndex)
            {
                return;
            }

            _pageIndex = prevIndex;
            SetPage(_pageIndex);
        }

        private void OnClickNext()
        {
            _pageIndex += 1;

            if (_pageIndex >= _pages.Length)
            {
                UIManager.Instance.CloseUI<DeckConstructionResultUI>();
                _pageIndex = 0;
                return;
            }
            
            SetPage(_pageIndex);
        }

        private void SetPage(int index)
        {
            btnPrev.gameObject.SetActive(index != 0);
            
            for (int i = 0; i < _pages.Length; i++)
            {
                _pages[i].SetActive(i == index);
            }
        }

        public override void OnOpen()
        {
            _pageIndex = 0;
            SetPage(_pageIndex);
        }

        public void SetResults(IReadOnlyCollection<DeckConstructionEvent> results)
        {
            playerSelectedClubListViewer.FetchData(results);
            
            var myResult = results.FirstOrDefault(result => result.Player.IsLocalPlayer);
            if (myResult == null)
            {
                Debug.LogWarning($"{nameof(DeckConstructionResultUI)} : Can't find local players DeckConstructionEvent");
                return;
            }
            
            deckConfigureCardsPage.SetSelectedClubs(myResult.SelectedClubFlag);
        }

        public override void Refresh()
        {
            foreach (var page in _pages)
            {
                page.Refresh();
            }
        }
    }
}
