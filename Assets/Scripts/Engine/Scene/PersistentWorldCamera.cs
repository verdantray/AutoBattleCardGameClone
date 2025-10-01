using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using ProjectABC.Utils;

namespace ProjectABC.Engine.Scene
{
    public sealed class PersistentWorldCamera : MonoSingleton<PersistentWorldCamera>
    {
        [Serializable]
        private class StrengthPoint
        {
            public float focusDistance;
            public float fStop;
        }
        
        [SerializeField] private Camera worldCamera;
        [SerializeField] private Volume volume;
        [SerializeField] private StrengthPoint neutralPoint;
        [SerializeField] private StrengthPoint targetPoint;
        [SerializeField] private AnimationCurve focusDistCurve;
        [SerializeField] private AnimationCurve apertureCurve;
        [SerializeField] private float fixedFocalLength;

        protected override bool SetPersistent => false;

        public Camera WorldCamera => worldCamera;
        
        private DepthOfField _depthOfField = null;
        private Coroutine _tweenRoutine = null;

        private void OnDisable()
        {
            StopTween();
        }

        private void OnDestroy()
        {
            StopTween();
        }

        protected override void Awake()
        {
            base.Awake();

            if (!IsInstance)
            {
                return;
            }
            
            InitialDoF();
        }

        private void InitialDoF()
        {
            if (!(bool)volume || !volume.profile.TryGet(out _depthOfField))
            {
                Debug.LogError($"{nameof(PersistentWorldCamera)} : need DepthOfField override on Volume Profile...");
                return;
            }

            _depthOfField.mode.value = DepthOfFieldMode.Bokeh;
            _depthOfField.active = true;
            
            SetStrength(0.0f);
        }

        public void BlurOn(float duration = 1.0f, bool useUnscaledTime = true, Action onComplete = null)
        {
            StopTween();
            _tweenRoutine = StartCoroutine(TweenStrength(0.0f, 1.0f, duration, useUnscaledTime, onComplete));
        }

        public void BlurOff(float duration = 1.0f, bool useUnscaledTime = true, Action onComplete = null)
        {
            StopTween();
            _tweenRoutine = StartCoroutine(TweenStrength(1.0f, 0.0f, duration, useUnscaledTime, onComplete));
        }

        public void BlurTo(float strength)
        {
            StopTween();
            SetStrength(strength);
        }

        private IEnumerator TweenStrength(float from, float to, float duration, bool useUnscaledTime, Action onComplete)
        {
            float elapsed = 0.0f;
            while (elapsed < duration)
            {
                elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                SetStrength(Mathf.Lerp(from, to, elapsed / duration));
                
                yield return null;
            }
            
            SetStrength(to);
            onComplete?.Invoke();
            
            _tweenRoutine = null;
        }

        private void StopTween()
        {
            if (_tweenRoutine != null)
            {
                StopCoroutine(_tweenRoutine);
            }

            _tweenRoutine = null;
        }

        private void SetStrength(float strength)
        {
            strength = Mathf.Clamp01(strength);
            
            float focusDistInterpolated =  focusDistCurve.Evaluate(strength);
            float fStopInterpolated = apertureCurve.Evaluate(strength);

            _depthOfField.focusDistance.value = Mathf.Lerp(
                neutralPoint.focusDistance,
                targetPoint.focusDistance,
                focusDistInterpolated
            );

            _depthOfField.aperture.value = Mathf.Lerp(
                neutralPoint.fStop,
                targetPoint.fStop,
                fStopInterpolated
            );

            _depthOfField.focalLength.value = fixedFocalLength;
        }
    }
}