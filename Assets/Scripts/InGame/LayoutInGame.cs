using System;
using System.Collections;
using System.Collections.Generic;
using ProjectABC.Core;
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
        public List<ButtonCardSelect> _buttonCardSelects;
        public Button _btnSelectCard;
        public Button _btnRerollCard;
        
        private int _debugWinCount;
        private int _debugLoseCount;
        
        private int _mulliganChance;
        private readonly List<Card> _cardSelected = new();

        private PlayerState _state; 
        private GradeType _gradeType;
        private int _amount;
        private List<Card> _cardsInHand = new();
        
        public Action<int> OnFinishRecruitLevelAmount;
        public Action<List<Card>> OnFinishDrawCard;

        private void Start()
        {
            _btnSelectCard.onClick.AddListener(FinishDrawCard);
            _btnRerollCard.onClick.AddListener(RerollCards);

            for (var i = 0; i < _buttonCardSelects.Count; i++)
            {
                var cardSelect = _buttonCardSelects[i];
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
        public void OnStartRecruitLevelAmount(IReadOnlyList<Tuple<GradeType, int>> pair)
        {
            for (var i = 0; i < _buttonCardPools.Count; i++)
            {
                var (gradeType, count) = pair[i];
                var button = _buttonCardPools[i];
                button.SetCardPool(gradeType, count);
            }
            
            StartCoroutine(ShowPanelCardPoolSelect());
        }
        public void OnSelectCardPool(int index)
        {
            OnFinishRecruitLevelAmount.Invoke(index);
            StartCoroutine(HidePanelCardPoolSelect());
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

        #region Draw Cards
        public void OnStartDrawCard(PlayerState state, GradeType gradeType, int amount)
        {
            _mulliganChance = 0;
            _cardSelected.Clear();
            _cardsInHand.Clear();
            
            _state = state;
            _gradeType = gradeType;
            _amount = amount;
            
            SetButtonCardSelects();
            StartCoroutine(ShowPanelCardSelect());
        }
        public void FinishDrawCard()
        {
            OnFinishDrawCard.Invoke(_cardSelected);
            StartCoroutine(HidePanelCardSelect());
        }
        public void RerollCards()
        {
            _mulliganChance += 1;
            
            CardPile targetCardPile = _state.GradeCardPiles[_gradeType];
            targetCardPile.AddRange(_cardsInHand);
            
            SetButtonCardSelects();
        }
        public void OnSelectCard(int index)
        {
            Card card = _buttonCardSelects[index].GetCard();
            if (_cardSelected.Contains(card))
            {
                _cardSelected.Remove(card);
            }
            else
            {
                if (_cardSelected.Count >= _amount)
                    return;
                
                _cardSelected.Add(card);
            }
            
            for (int i = 0; i < _buttonCardSelects.Count; i++)
            {
                var targetCard = _buttonCardSelects[i].GetCard();
                var isSelected = _cardSelected.Contains(targetCard);
                _buttonCardSelects[i].SetSelected(isSelected);
            }
            
            var hasMulliganChance = _mulliganChance == _state.MulliganChances;
            if (hasMulliganChance)
            {
                _btnRerollCard.interactable = false;
            }
            else
            {
                _btnRerollCard.interactable = _cardSelected.Count == 0;
            }
            
            var isCardSelectedAll = _cardSelected.Count == _amount;
            _btnSelectCard.interactable = isCardSelectedAll;
        }
        private void SetButtonCardSelects()
        {
            // TODO: CardPile에서 뽑고 안쓴 카드 넣는 식으로
            CardPile targetCardPile = _state.GradeCardPiles[_gradeType];
            int handAmount = GameConst.GameOption.RECRUIT_HAND_AMOUNT;
            _cardsInHand = targetCardPile.DrawCards(handAmount);

            for (int i = 0; i < handAmount; i++)
            {
                var button = _buttonCardSelects[i];
                var cardData = _cardsInHand[i];
                
                button.SetCard(cardData);
                button.SetSelected(false);
            }

            var hasMulliganChance = _mulliganChance == _state.MulliganChances;
            _btnRerollCard.interactable = hasMulliganChance;
            
            _btnSelectCard.interactable = false;
        }
        #endregion

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
            _panelCardSelect.interactable = true;
            for (var t = 0f; t <= 1f; t += Time.deltaTime / TimeToShowSplash)
            {
                _panelCardSelect.alpha = t;
                yield return null;
            }
            _panelCardSelect.alpha = 1f;
        }
        public IEnumerator HidePanelCardSelect()
        {
            _panelCardSelect.interactable = true;
            for (var t = 0f; t <= 1f; t += Time.deltaTime / TimeToHideSplash)
            {
                _panelCardSelect.alpha = 1f - t;
                yield return null;
            }
            _panelCardSelect.alpha = 0f;
        }
    }
}