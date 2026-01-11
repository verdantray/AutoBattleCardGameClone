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
    public sealed class SelectCardsUI : UIElement
    {
        [SerializeField] private SelectCardToggle[] toggles;
        [SerializeField] private float toggleInterval;
        [SerializeField] private Button btnReroll;
        [SerializeField] private Button btnSelect;
        [SerializeField] private TextMeshProUGUI txtReroll;
        [SerializeField] private TextMeshProUGUI txtSelect;

        private readonly List<string> _drawnCardIds = new List<string>();
        private readonly List<string> _selectedCardIds = new List<string>();
        private readonly List<string> _rejectedCardIds = new List<string>();
        
        private Vector2 _referenceResolution;
        private SelectCardsParam _param;
        private Coroutine _refreshRoutine;

        private void Awake()
        {
            foreach (var toggle in toggles)
            {
                toggle.AddListener(RefreshSelectButton);
            }
            
            btnSelect.onClick.AddListener(OnSelect);
            btnReroll.onClick.AddListener(OnReroll);
        }

        private void OnDisable()
        {
            StopRoutine();
        }

        private void OnDestroy()
        {
            StopRoutine();
            
            btnSelect.onClick.RemoveAllListeners();
            btnReroll.onClick.RemoveAllListeners();
        }

        private void OnSelect()
        {
            float pickedCardDestY = (_referenceResolution.y + (toggles[0].Size.y * 2.0f)) * -0.5f;
            PickCards(_selectedCardIds, pickedCardDestY);

            if (_selectedCardIds.Count == _param.SelectCardAmount)
            {
                UIManager.Instance.CloseUI<SelectCardsUI>();
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
            PickCards(_rejectedCardIds, pickedCardDestY);
            
            StopRoutine();
            _refreshRoutine = StartCoroutine(DelayedRefresh(0.15f));
        }

        private void PickCards(ICollection<string> pickedCards, float togglePosY)
        {
            var toggledIndexes = GetToggledIndexes();

            for (int i = toggledIndexes.Length - 1; i >= 0; i--)
            {
                int index = toggledIndexes[i];
                var card = _drawnCardIds[index];
                _drawnCardIds.RemoveAt(index);
                pickedCards.Add(card);
            }
            
            foreach (var toggle in toggles)
            {
                toggle.IsOn = false;
            }
            
            int cardsCount = _drawnCardIds.Count;
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

        public void SetSelectParam(SelectCardsParam param)
        {
            _param = param;
            Refresh();
        }
        
        public override void Refresh()
        {
            var (amount, cardIdQueue, rerollChance) = _param;
            
            btnSelect.interactable = false;
            btnReroll.interactable = false;

            cardIdQueue.EnqueueCardIds(_rejectedCardIds);
            _rejectedCardIds.Clear();
            
            int prevExistsCount = _drawnCardIds.Count;

            if (_drawnCardIds.Count < GameConst.GameOption.DEFAULT_CARD_SELECT_AMOUNT)
            {
                int drawSize = GameConst.GameOption.DEFAULT_CARD_SELECT_AMOUNT - _drawnCardIds.Count;
                var drawnCardIds = rerollChance.GetRerollCardIds(cardIdQueue, drawSize);
                
                _drawnCardIds.AddRange(drawnCardIds);
            }

            int cardsCount = _drawnCardIds.Count;
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
                
                toggles[i].SetCard(_drawnCardIds[i]);
                toggles[i].MoveToPosition(from, target, delay, duration, callback);
            }
            
            RefreshSelectButton(false);
            RefreshRerollButton();
        }

        private void RefreshSelectButton(bool _)
        {
            var toggledIndexes = GetToggledIndexes();
            btnSelect.interactable = _selectedCardIds.Count + toggledIndexes.Length <= _param.SelectCardAmount;
            
            txtSelect.text = LocalizationHelper.Instance.Localize(
                "ui_select_cards_select",
                toggledIndexes.Length + _selectedCardIds.Count,
                _param.SelectCardAmount
            );
        }

        private void RefreshRerollButton()
        {
            txtReroll.text = LocalizationHelper.Instance.Localize(
                "ui_select_cards_reroll",
                _param.RerollChance.RemainRerollChance
            );
            
            btnReroll.interactable = _param.RerollChance.RemainRerollChance > 0;
        }

        public async Task<List<string>> GetSelectCardIdsAsync()
        {
            await WaitUntilCloseAsync();

            List<string> cards = new List<string>(_selectedCardIds);
            _selectedCardIds.Clear();

            _param.CardIdQueue.EnqueueCardIds(_drawnCardIds);
            _drawnCardIds.Clear();
            
            _param.RerollChance.Reset();
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

        public class SelectCardsParam
        {
            public readonly int SelectCardAmount;
            public readonly CardIdQueue CardIdQueue;
            public readonly RerollChance RerollChance;

            public SelectCardsParam(int amount, CardIdQueue cardIdQueue, RerollChance rerollChance)
            {
                SelectCardAmount = amount;
                CardIdQueue = cardIdQueue;
                RerollChance = rerollChance;
            }

            public void Deconstruct(out int amount, out CardIdQueue cardIdQueue, out RerollChance rerollChance)
            {
                amount = SelectCardAmount;
                cardIdQueue = CardIdQueue;
                rerollChance = RerollChance;
            }
        }

        #endregion
    }
}
