using System;
using System.Collections;
using ProjectABC.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ProjectABC.Engine.UI
{
    public sealed class RecruitCardToggle : MonoBehaviour
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Toggle toggle;
        [SerializeField] private CardUIItem cardUIItem;

        public bool IsOn
        {
            get => toggle.isOn;
            set => toggle.isOn = value;
        }
        
        public Vector2 Size => rectTransform.sizeDelta;
        public Vector2 AnchoredPosition => rectTransform.anchoredPosition;
        
        private Coroutine _moveRoutine;

        private void Awake()
        {
            toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        private void OnDisable()
        {
            StopMoveRoutine();
        }

        private void OnDestroy()
        {
            toggle.onValueChanged.RemoveAllListeners();
            StopMoveRoutine();
        }

        public void AddListener(UnityAction<bool> callback)
        {
            toggle.onValueChanged.AddListener(callback);
        }

        private void OnToggleValueChanged(bool isOn)
        {
            Vector2 position = rectTransform.anchoredPosition + ((isOn ? Vector2.up : Vector2.down) * 120.0f);
            MoveToPosition(position);
        }

        public void SetCard(Card card)
        {
            cardUIItem.ApplyData(new CardReference(card.Id));
        }

        public void MoveToPosition(Vector2 from, Vector2 target, float delay = 0.0f, float duration = 0.05f, Action callback = null)
        {
            StopMoveRoutine();
            _moveRoutine = StartCoroutine(MoveToPositionRoutine(from, target, delay, duration, callback));
        }

        public void MoveToPosition(Vector2 target, float delay = 0.0f, float duration = 0.05f, Action callback = null)
        {
            StopMoveRoutine();
            
            Vector2 from = rectTransform.anchoredPosition;
            _moveRoutine = StartCoroutine(MoveToPositionRoutine(from, target, delay, duration, callback));
        }

        private void StopMoveRoutine()
        {
            if (_moveRoutine != null)
            {
                StopCoroutine(_moveRoutine);
            }
            
            _moveRoutine = null;
        }

        private IEnumerator MoveToPositionRoutine(Vector2 from, Vector2 target, float delay, float duration, Action callback)
        {
            rectTransform.anchoredPosition = from;
            toggle.interactable = false;

            if (delay > 0.0f)
            {
                yield return new WaitForSeconds(delay);
            }

            if (duration > 0.0f)
            {
                float elapsed = 0.0f;
                while (elapsed < duration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    rectTransform.anchoredPosition = Vector2.Lerp(from, target, elapsed / duration);

                    yield return null;
                }
            }

            rectTransform.anchoredPosition = target;
            toggle.interactable = true;
            
            callback?.Invoke();
            _moveRoutine = null;
        }
    }
}
