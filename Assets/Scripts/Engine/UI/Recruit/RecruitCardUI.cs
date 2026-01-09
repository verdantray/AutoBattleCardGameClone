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

        private readonly List<string> _drawnCardIds = new List<string>();
        private readonly List<string> _recruitCardIds = new List<string>();
        private readonly List<string> _rejectedCardIds = new List<string>();
        
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
            PickCards(_recruitCardIds, pickedCardDestY);

            if (_recruitCardIds.Count == _param.RecruitCardAmount)
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
            
            int cardsCount = _drawnCardIds.Count + _rejectedCardIds.Count;
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
            var (amount, cardIdQueue, rerollChance) = _param;
            
            btnRecruit.interactable = false;
            btnReroll.interactable = false;

            int prevExistsCount = _drawnCardIds.Count;

            if (_drawnCardIds.Count < GameConst.GameOption.RECRUIT_HAND_AMOUNT)
            {
                int drawSize = GameConst.GameOption.RECRUIT_HAND_AMOUNT - _drawnCardIds.Count;
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
        }

        private void RefreshRecruitButton(bool _)
        {
            var toggledIndexes = GetToggledIndexes();
            btnRecruit.interactable = _recruitCardIds.Count + toggledIndexes.Length <= _param.RecruitCardAmount;
            
            txtRecruit.text = LocalizationHelper.Instance.Localize(
                "ui_recruit_card_recruit_students",
                toggledIndexes.Length + _recruitCardIds.Count,
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

        public async Task<List<string>> GetRecruitCardsAsync()
        {
            await WaitUntilCloseAsync();

            _param.CardIdQueue.EnqueueCardIds(_recruitCardIds);
            _param.CardIdQueue.EnqueueCardIds(_drawnCardIds);

            List<string> cards = new List<string>(_recruitCardIds);
            _recruitCardIds.Clear();

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

        public class RecruitCardParam
        {
            public readonly int RecruitCardAmount;
            public readonly CardIdQueue CardIdQueue;
            public readonly RerollChance RerollChance;

            public RecruitCardParam(int amount, CardIdQueue cardIdQueue, RerollChance rerollChance)
            {
                RecruitCardAmount = amount;
                CardIdQueue = cardIdQueue;
                RerollChance = rerollChance;
            }

            public void Deconstruct(out int amount, out CardIdQueue cardIdQueue, out RerollChance rerollChance)
            {
                amount = RecruitCardAmount;
                cardIdQueue = CardIdQueue;
                rerollChance = RerollChance;
            }
        }

        #endregion
    }
}
