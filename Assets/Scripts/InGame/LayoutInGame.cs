using System;
using System.Collections;
using System.Collections.Generic;
using ProjectABC.Core;
using ProjectABC.Data;
using ProjectABC.InGame.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        public TextMeshProUGUI _textMatchEndReasonWin;
        public TextMeshProUGUI _textMatchEndReasonLose;
        
        public CanvasGroup _splashStart;
        public TextMeshProUGUI _textRound;
        public TextMeshProUGUI _textMatch;
        
        [Header("Select Card Pool")] 
        public CanvasGroup _panelCardPoolSelect;
        public List<ButtonCardPool> _buttonCardPools;
        
        [Header("Select Card")] 
        public CanvasGroup _panelCardSelect;
        public List<ButtonCardSelect> _buttonCardSelects;
        public Button _btnSelectCard;
        public TextMeshProUGUI _textSelectCard;
        public Button _btnRerollCard;
        public TextMeshProUGUI _textRerollCard;
        
        private int _debugWinCount;
        private int _debugLoseCount;
        
        private int _mulliganChance;
        private readonly List<Card> _cardSelected = new();

        private PlayerState _state; 
        private GradeType _gradeType;
        private int _amount;
        private List<Card> _cardsInHand = new();
        private List<Card> _cardsSelectedInHand = new();
        
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
            foreach (var button in _buttonCardPools)
            {
                button.gameObject.SetActive(false);
            }
            for (var i = 0; i < pair.Count; i++)
            {
                var (gradeType, count) = pair[i];
                var button = _buttonCardPools[i];
                button.SetCardPool(gradeType, count);
                button.gameObject.SetActive(true);
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
            _cardsSelectedInHand.Clear();
            
            _state = state;
            _gradeType = gradeType;
            _amount = amount;
            
            var hasMulliganChance = _mulliganChance < _state.MulliganChances;
            _btnRerollCard.gameObject.SetActive(hasMulliganChance);
            _btnRerollCard.interactable = hasMulliganChance;
            
            CardPile targetCardPile = _state.GradeCardPiles[_gradeType];
            int handAmount = GameConst.GameOption.RECRUIT_HAND_AMOUNT;
            _cardsInHand = targetCardPile.DrawCards(handAmount);
            
            SetButtonCardSelects(_cardsInHand);
            StartCoroutine(ShowPanelCardSelect());
        }
        public void FinishDrawCard()
        {
            _cardSelected.AddRange(_cardsSelectedInHand);
            foreach (var card in _cardsSelectedInHand)
            {
                _cardsInHand.Remove(card);
            }
            _cardsSelectedInHand.Clear();
            SetButtonCardSelects(_cardsInHand);
            
            if (_cardSelected.Count == _amount)
            {
                OnFinishDrawCard.Invoke(_cardSelected);
                StartCoroutine(HidePanelCardSelect());
            }
        }
        public void RerollCards()
        {
            _mulliganChance += 1;
            
            var hasMulliganChance = _mulliganChance < _state.MulliganChances;
            _btnRerollCard.gameObject.SetActive(hasMulliganChance);
            
            CardPile targetCardPile = _state.GradeCardPiles[_gradeType];

            foreach (var card in _cardsInHand)
            {
                targetCardPile.Add(card);
            }
            
            int handAmount = GameConst.GameOption.RECRUIT_HAND_AMOUNT - _cardSelected.Count;
            _cardsInHand = targetCardPile.DrawCards(handAmount);
            SetButtonCardSelects(_cardsInHand);
        }
        public void OnSelectCard(int index)
        {
            int cardAmount = _amount - _cardSelected.Count;
            
            Card card = _buttonCardSelects[index].GetCard();
            if (_cardsSelectedInHand.Contains(card))
            {
                _cardsSelectedInHand.Remove(card);
            }
            else
            {
                if (_cardsSelectedInHand.Count >= cardAmount)
                    return;
                
                _cardsSelectedInHand.Add(card);
            }
            
            for (int i = 0; i < _buttonCardSelects.Count; i++)
            {
                var targetCard = _buttonCardSelects[i].GetCard();
                var isSelected = _cardsSelectedInHand.Contains(targetCard);
                _buttonCardSelects[i].SetSelected(isSelected);
            }

            var isCardSelected = _cardsSelectedInHand.Count > 0;
            _btnSelectCard.interactable = isCardSelected;

            var hasMulliganChance = _mulliganChance < _state.MulliganChances;
            _btnRerollCard.interactable = !isCardSelected && hasMulliganChance;
        }
        private void SetButtonCardSelects(List<Card> cards)
        {
            // TODO: CardPile에서 뽑고 안쓴 카드 넣는 식으로
            foreach (var button in _buttonCardSelects)
            {
                button.gameObject.SetActive(false);
            }
            
            int handAmount = GameConst.GameOption.RECRUIT_HAND_AMOUNT - _cardSelected.Count;
            for (int i = 0; i < handAmount; i++)
            {
                var button = _buttonCardSelects[i];
                var cardData = cards[i];
                
                button.SetCard(cardData);
                button.SetSelected(false);
                button.gameObject.SetActive(true);
            }

            var hasMulliganChance = _mulliganChance < _state.MulliganChances;
            var leftMulliganChance = _state.MulliganChances - _mulliganChance;
            _btnRerollCard.interactable = hasMulliganChance;
            _textRerollCard.text = $"Reroll({leftMulliganChance})";
            
            var leftCardCount = _amount - _cardSelected.Count;
            _btnSelectCard.interactable = false;
            _textSelectCard.text = $"Select({leftCardCount})";
        }
        #endregion

        public void OnStartRound(int round, string playerA, string playerB)
        {
            _textRound.text = $"Round {round}";
            _textMatch.text = $"{playerA} vs {playerB}";
            StartCoroutine(ShowSplash(_splashStart));
        }
        
        public void OnEndRound(MatchState matchState, MatchEndReason reason)
        {
            var stringReason = "";
            switch (reason)
            {
                case MatchEndReason.EndByEmptyDeck:
                    stringReason = "덱의 모든 카드를 소진하였습니다.";
                    break;
                
                case MatchEndReason.EndByFullOfInfirmary:
                    stringReason = "양호실에 남은 슬롯이 없습니다.";
                    break;
            }
            
            switch (matchState)
            {
                case MatchState.Attacking:
                {
                    _debugWinCount++;
                    _textMatchEndReasonWin.text = stringReason;
                    StartCoroutine(ShowSplash(_splashWin));
                }
                break;

                case MatchState.Defending:
                {
                    _debugLoseCount++;
                    _textMatchEndReasonLose.text = stringReason;
                    StartCoroutine(ShowSplash(_splashLose));
                }
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