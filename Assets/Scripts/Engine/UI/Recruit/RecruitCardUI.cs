using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectABC.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectABC.Engine.UI
{
    public sealed class RecruitCardUI : UIElement
    {
        [SerializeField] private RecruitCardToggle[] toggles;
        [SerializeField] private float toggleInterval;
        [SerializeField] private Button btnReroll;
        [SerializeField] private Button btnRecruit;
        [SerializeField] private TextMeshProUGUI txtReroll;
        [SerializeField] private TextMeshProUGUI txtRecruit;

        private readonly List<Card> _drawnCards = new List<Card>();
        private readonly List<Card> _recruitCards = new List<Card>();
        private readonly List<Card> _rejectedCards = new List<Card>();
        
        private Vector2 _referenceResolution;
        private RecruitCardParam _param;
        private Coroutine _refreshRoutine;

        private void Awake()
        {
            foreach (var toggle in toggles)
            {
                toggle.AddListener(RefreshRecruitButton);
            }
            
            btnRecruit.onClick.AddListener(OnRecruit);
            btnReroll.onClick.AddListener(OnReroll);
        }

        private void OnDisable()
        {
            StopRoutine();
        }

        private void OnDestroy()
        {
            StopRoutine();
            
            btnRecruit.onClick.RemoveAllListeners();
            btnReroll.onClick.RemoveAllListeners();
        }

        private void OnRecruit()
        {
            float pickedCardDestY = (_referenceResolution.y + (toggles[0].Size.y * 2.0f)) * -0.5f;
            PickCards(_recruitCards, pickedCardDestY);

            if (_recruitCards.Count == _param.RecruitCardAmount)
            {
                UIManager.Instance.CloseUI<RecruitCardUI>();
            }
            else
            {
                StopRoutine();
                _refreshRoutine = StartCoroutine(DelayedRefresh(0.15f));
            }
        }

        private void OnReroll()
        {
            float pickedCardDestY = (_referenceResolution.y + (toggles[0].Size.y * 2.0f)) * 0.5f;
            PickCards(_rejectedCards, pickedCardDestY);
            
            StopRoutine();
            _refreshRoutine = StartCoroutine(DelayedRefresh(0.15f));
        }

        private void PickCards(ICollection<Card> pickedCards, float togglePosY)
        {
            var toggledIndexes = GetToggledIndexes();

            for (int i = toggledIndexes.Length - 1; i >= 0; i--)
            {
                int index = toggledIndexes[i];
                var card = _drawnCards[index];
                _drawnCards.RemoveAt(index);
                pickedCards.Add(card);
            }
            
            foreach (var toggle in toggles)
            {
                toggle.IsOn = false;
            }
            
            int cardsCount = _drawnCards.Count + _rejectedCards.Count;
            bool isOdd = cardsCount % 2 != 0;
            float left = (toggleInterval * Mathf.FloorToInt(cardsCount / 2.0f) * -1.0f)
                         + (isOdd ? 0.0f : toggleInterval * 0.5f);
            
            for (int i = 0; i < toggles.Length; i++)
            {
                Vector2 from = toggles[i].AnchoredPosition;
                Vector2 target = from;

                bool isReroll = toggledIndexes.Contains(i);
                
                if (isReroll)
                {
                    target.y = togglePosY;
                }
                else
                {
                    int order = i - toggledIndexes.Count(index => index < i);
                    target.x = left + (toggleInterval * order);
                }
                
                float delay = isReroll ? 0.0f : 0.1f;
                toggles[i].MoveToPosition(target, delay);
            }
        }

        private void StopRoutine()
        {
            if (_refreshRoutine != null)
            {
                StopCoroutine(_refreshRoutine);
            }

            _refreshRoutine = null;
        }

        private IEnumerator DelayedRefresh(float delay)
        {
            yield return new WaitForSeconds(delay);
            Refresh();
        }

        public override void OnOpen()
        {
            CanvasScaler canvasScaler = UIManager.Instance.Canvas.GetComponent<CanvasScaler>();
            _referenceResolution = canvasScaler.referenceResolution;
        }

        public void SetRecruit(RecruitCardParam param)
        {
            _param = param;
            Refresh();
        }
        
        public override void Refresh()
        {
            var (amount, cardPile, rerollChance) = _param;
            
            btnRecruit.interactable = false;
            btnReroll.interactable = false;

            int prevExistsCount = _drawnCards.Count;

            if (_drawnCards.Count < GameConst.GameOption.RECRUIT_HAND_AMOUNT)
            {
                int drawSize = GameConst.GameOption.RECRUIT_HAND_AMOUNT - _drawnCards.Count;
                var cards = rerollChance.GetRerollCards(cardPile, drawSize);
                
                _drawnCards.AddRange(cards);
            }

            int cardsCount = _drawnCards.Count;
            bool isOdd = cardsCount % 2 != 0;
            float left = (toggleInterval * Mathf.FloorToInt(cardsCount / 2.0f) * -1.0f)
                         + (isOdd ? 0.0f : toggleInterval * 0.5f);
            
            for (int i = 0; i < cardsCount; i++)
            {
                bool alreadySet = i < prevExistsCount;

                Vector2 from = Vector2.zero;
                from.x = alreadySet
                    ? left + (toggleInterval * i)
                    : (_referenceResolution.x + toggles[i].Size.x) * 0.5f;
                
                Vector2 target = Vector2.zero;
                target.x = left + (toggleInterval * i);

                float delay = alreadySet ? 0.0f : 0.1f * (i - prevExistsCount);
                float duration = alreadySet ? 0.0f : 0.05f;
                
                bool isLast =  i == cardsCount - 1;
                Action callback = isLast ? RefreshRerollButton : null;
                
                toggles[i].SetCard(_drawnCards[i]);
                toggles[i].MoveToPosition(from, target, delay, duration, callback);
            }
        }

        private void RefreshRecruitButton(bool _)
        {
            var toggledIndexes = GetToggledIndexes();
            btnRecruit.interactable = _recruitCards.Count + toggledIndexes.Length <= _param.RecruitCardAmount;
            
            txtRecruit.text = LocalizationHelper.Instance.Localize(
                "ui_recruit_card_recruit_students",
                toggledIndexes.Length + _recruitCards.Count,
                _param.RecruitCardAmount
            );
        }

        private void RefreshRerollButton()
        {
            txtReroll.text = LocalizationHelper.Instance.Localize(
                "ui_recruit_card_reroll",
                _param.RerollChance.RemainRerollChance
            );
            
            btnReroll.interactable = _param.RerollChance.RemainRerollChance > 0;
        }

        public async Task<List<Card>> GetRecruitCardsAsync()
        {
            await WaitUntilCloseAsync();

            foreach (var card in _rejectedCards)
            {
                _param.CardPile.Add(card);
            }

            foreach (var card in _drawnCards)
            {
                _param.CardPile.Add(card);
            }

            List<Card> cards = new List<Card>(_recruitCards);
            _recruitCards.Clear();

            _param = null;

            return cards;
        }

        private int[] GetToggledIndexes()
        {
            int[] toggledIndex = toggles
                .Where(toggle => toggle.IsOn)
                .Select(toggle => Array.IndexOf(toggles, toggle))
                .ToArray();
            
            return toggledIndex;
        }

        #region inner parameter class for functionality

        public class RecruitCardParam
        {
            public readonly int RecruitCardAmount;
            public readonly CardPile CardPile;
            public readonly RerollChance RerollChance;

            public RecruitCardParam(int amount, CardPile cardPile, RerollChance rerollChance)
            {
                RecruitCardAmount = amount;
                CardPile = cardPile;
                RerollChance = rerollChance;
            }

            public void Deconstruct(out int amount, out CardPile cardPile, out RerollChance rerollChance)
            {
                amount = RecruitCardAmount;
                cardPile = CardPile;
                rerollChance = RerollChance;
            }
        }

        #endregion
    }
}
