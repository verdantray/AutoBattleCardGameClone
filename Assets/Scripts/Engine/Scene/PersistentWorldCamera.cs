using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using ProjectABC.Utils;

namespace ProjectABC.Engine.Scene
{
    public sealed class PersistentWorldCamera : MonoSingleton<PersistentWorldCamera>
    {
        private const float MIN_FOCUS_DIST = 0.1f;
        private const float MIN_FOCAL_LEN = 1.0f;
        
        [SerializeField] private Camera worldCamera;
        [SerializeField] private Volume volume;
        [SerializeField][Min(MIN_FOCUS_DIST)] private float focusDistance;
        [SerializeField][Range(1.0f, 300.0f)] private float maxFocalLength;
        [SerializeField] private AnimationCurve curve;      // will replace to DoTween

        protected override bool SetPersistent => false;

        public Camera WorldCamera => worldCamera;
        
        private DepthOfField _depthOfField;
        private Coroutine _tweenRoutine;

        private void OnDisable()
        {
            if (_tweenRoutine != null)
            {
                StopCoroutine(_tweenRoutine);
                _tweenRoutine = null;
            }
        }

        private void OnDestroy()
        {
            if (_tweenRoutine != null)
            {
                StopCoroutine(_tweenRoutine);
                _tweenRoutine = null;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            if (!IsInstance)
            {
                return;
            }
            
            if (volume == null || !volume.profile.TryGet(out _depthOfField))
            {
                Debug.LogError($"{nameof(PersistentWorldCamera)} : need DepthOfField override on Volume Profile...");
                return;
            }

            _depthOfField.mode.value = DepthOfFieldMode.Bokeh;
            _depthOfField.focusDistance.value = focusDistance;
            _depthOfField.focusDistance.value = MIN_FOCAL_LEN;
        }

        private async void Start()
        {
            await Task.Delay(2 * 1000);
            
            Debug.Log("BlurOn");
            BlurOn();

            await Task.Delay(2 * 1000);
            
            Debug.Log("BlurOff");
            BlurOff();
        }

        public void BlurOn(float duration = 1.0f, Action onComplete = null, bool useUnscaledTime = true)
            => BlurTo(maxFocalLength, duration, onComplete, useUnscaledTime);

        public void BlurOff(float duration = 1.0f, Action onComplete = null, bool useUnscaledTime = true)
            => BlurTo(MIN_FOCAL_LEN, duration, onComplete, useUnscaledTime);

        public void BlurTo(float targetLength, float duration, Action onComplete, bool useUnscaledTime)
        {
            if (_tweenRoutine != null)
            {
                StopCoroutine(_tweenRoutine);
            }

            float currentFocalLength = _depthOfField.focalLength.value;
            _tweenRoutine = StartCoroutine(TweenBlur(currentFocalLength, targetLength, duration, onComplete, useUnscaledTime));
        }

        private IEnumerator TweenBlur(float from, float to, float duration, Action onComplete, bool useUnscaledTime)
        {
            if (duration <= 0.0f)
            {
                OnComplete();
                yield break;
            }

            float normalizedTime = 0.0f;
            while (normalizedTime < 1.0f)
            {
                float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                normalizedTime += deltaTime / duration;

                float interpolated = curve.Evaluate(normalizedTime);
                _depthOfField.focalLength.value = Mathf.Clamp(interpolated, from, to);
                
                yield return null;
            }
            
            _tweenRoutine = null;
            yield break;

            void OnComplete()
            {
                _depthOfField.gaussianMaxRadius.value = to;
                onComplete?.Invoke();
            }
        }
    }
}