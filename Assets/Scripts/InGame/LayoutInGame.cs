using System;
using System.Collections;
using System.Collections.Generic;
using ProjectABC.Data;
using ProjectABC.InGame.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace ProjectABC.InGame
{
    public class LayoutInGame : LayoutBase
    {
        private const float TimeToShowSplash = 0.5f;
        private const float TimeToWaitSplash = 1f;
        private const float TimeToHideSplash = 0.5f;
        
        public TextMeshProUGUI _textScore;
        
        public CanvasGroup _splashWin;
        public CanvasGroup _splashLose;
        
        
        [Header("Select Card Pool")] 
        public CanvasGroup _panelCardPoolSelect;
        public List<ButtonCardPool> _buttonCardPools;
        
        [Header("Select Card")] 
        public CanvasGroup _panelCardSelect;
        public List<CardInGameSelect> _cardInGameSelects;
        public Button _btnSelectCard;
        public Button _btnRerollCard;
        
        private int _debugWinCount;
        private int _debugLoseCount;
        
        private int _selectedCardPoolIndex = -1;
        private int _selectedCardIndex = -1;
        
        public Action<int> OnFinishRecruitLevelAmount;
        public Action<CardDataOld> OnFinishSelectCard;

        private readonly List<CardDataOld> _targetCardDataPool = new();

        private void Start()
        {
            _btnSelectCard.onClick.AddListener(FinishSelectCards);
            _btnRerollCard.onClick.AddListener(RerollCards);

            for (var i = 0; i < _cardInGameSelects.Count; i++)
            {
                var cardSelect = _cardInGameSelects[i];
                cardSelect.SetIndex(i);
                cardSelect.OnSelectCard += OnSelectCard;
            }
            
            for (var i = 0; i < _buttonCardPools.Count; i++)
            {
                var cardSelect = _buttonCardPools[i];
                cardSelect.SetIndex(i);
                cardSelect.OnSelectCardPool += OnSelectCardPool;
            }
        }

        #region Recruit Level Amount
        public void OnStartRecruitLevelAmount(IReadOnlyList<Tuple<LevelType, int>> pair)
        {
            for (var i = 0; i < _buttonCardPools.Count; i++)
            {
                var (levelType, count) = pair[i];
                var button = _buttonCardPools[i];
                button.SetCardPool(levelType, count);
            }
            
            StartCoroutine(ShowPanelCardPoolSelect());
        }
        public void FinishRecruitLevelAmount()
        {
            OnFinishRecruitLevelAmount.Invoke(_selectedCardPoolIndex);
            StartCoroutine(HidePanelCardPoolSelect());
        }
        public void OnSelectCardPool(int index)
        {
            _selectedCardPoolIndex = index;
            FinishRecruitLevelAmount();
        }
        private IEnumerator ShowPanelCardPoolSelect()
        {
            for (var t = 0f; t <= 1f; t += Time.deltaTime / TimeToShowSplash)
            {
                _panelCardPoolSelect.alpha = t;
                yield return null;
            }
            _panelCardPoolSelect.alpha = 1f;
            _panelCardPoolSelect.interactable = true;
            _panelCardPoolSelect.blocksRaycasts = true;
        }
        private IEnumerator HidePanelCardPoolSelect()
        {
            _panelCardPoolSelect.interactable = false;
            _panelCardPoolSelect.blocksRaycasts = false;
            for (var t = 0f; t <= 1f; t += Time.deltaTime / TimeToHideSplash)
            {
                _panelCardPoolSelect.alpha = 1f - t;
                yield return null;
            }
            _panelCardPoolSelect.alpha = 0f;
        }
        #endregion
        
        public void OnStartSelectCards(List<CardDataOld> cards)
        {
            _targetCardDataPool.Clear();
            _targetCardDataPool.AddRange(cards);
            
            foreach (var cardSelect in _cardInGameSelects)
            {
                var j = Random.Range(0, _targetCardDataPool.Count);
                var cardData = _targetCardDataPool[j];
                cardSelect.SetCard(cardData);
                cardSelect.SetSelected(false);
            }
            
            _btnSelectCard.interactable = false;
            
            StartCoroutine(ShowPanelCardSelect());
        }
        public void FinishSelectCards()
        {
            var cardData = _targetCardDataPool[_selectedCardIndex];
            OnFinishSelectCard.Invoke(cardData);
            
            StartCoroutine(HidePanelCardSelect());
        }
        public void RerollCards()
        {
            
        }
        public void OnSelectCard(int index)
        {
            foreach (var cardSelect in _cardInGameSelects)
            {
                cardSelect.SetSelected(false);
            }
            _cardInGameSelects[index].SetSelected(true);
            
            _selectedCardIndex = index;
            _btnSelectCard.interactable = index >= 0;
        }
        
        
        public void OnEndRound(PlayerType playerType)
        {
            switch (playerType)
            {
                case PlayerType.Player:
                    _debugWinCount++;
                    StartCoroutine(ShowSplash(_splashWin));
                    break;

                case PlayerType.Enemy:
                    _debugLoseCount++;
                    StartCoroutine(ShowSplash(_splashLose));
                    break;
            }

            _textScore.text = $"{_debugWinCount} : {_debugLoseCount}";
        }

        public IEnumerator ShowSplash(CanvasGroup canvasGroup)
        {
            for (var t = 0f; t <= 1f; t += Time.deltaTime / TimeToShowSplash)
            {
                canvasGroup.alpha = t;
                yield return null;
            }
            canvasGroup.alpha = 1f;

            yield return new WaitForSeconds(TimeToWaitSplash);
            
            for (var t = 0f; t <= 1f; t += Time.deltaTime / TimeToHideSplash)
            {
                canvasGroup.alpha = 1f - t;
                yield return null;
            }
            canvasGroup.alpha = 0f;
        }
        public IEnumerator ShowPanelCardSelect()
        {
            for (var t = 0f; t <= 1f; t += Time.deltaTime / TimeToShowSplash)
            {
                _panelCardSelect.alpha = t;
                yield return null;
            }
            _panelCardSelect.alpha = 1f;
        }
        public IEnumerator HidePanelCardSelect()
        {
            for (var t = 0f; t <= 1f; t += Time.deltaTime / TimeToHideSplash)
            {
                _panelCardSelect.alpha = 1f - t;
                yield return null;
            }
            _panelCardSelect.alpha = 0f;
        }
    }
}