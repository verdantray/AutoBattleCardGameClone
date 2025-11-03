using System;
using System.Collections;
using ProjectABC.Engine.Scene.PostFX;
using UnityEngine;
using UnityEngine.Rendering;
using ProjectABC.Utils;
using Unity.Cinemachine;
using UnityEngine.Rendering.Universal;

namespace ProjectABC.Engine.Scene
{
    public sealed class PersistentWorldCamera : MonoSingleton<PersistentWorldCamera>
    {
        [Serializable]
        private class BandFeather
        {
            public float bandHalfWidth;
            public float feather;

            public static BandFeather Lerp(BandFeather from, BandFeather to, float t)
            {
                return new BandFeather
                {
                    bandHalfWidth = Mathf.Lerp(from.bandHalfWidth, to.bandHalfWidth, t),
                    feather = Mathf.Lerp(from.feather, to.feather, t)
                };
            }
        }
        
        [SerializeField] private Camera worldCamera;
        [SerializeField] private Camera uiCamera;
        [SerializeField] private Volume volume;
        [SerializeField] private CinemachineBrain cineMachineBrain;
        [SerializeField] private BandFeather neutralBandFeather;
        [SerializeField] private BandFeather targetBandFeather;
        [SerializeField] private AnimationCurve blurCurve;
        
        protected override bool SetPersistent => false;

        public Camera WorldCamera => worldCamera;
        public Camera UICamera => uiCamera;

        public float DefaultBlendDuration
        {
            get => cineMachineBrain.DefaultBlend.Time;
            set => cineMachineBrain.DefaultBlend.Time = value;
        }

        private TiltShift _tiltShift;
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

            if (IsInstance)
            {
                InitialTiltShift();
            }
        }

        private void InitialTiltShift()
        {
            if (volume == null || !volume.profile.TryGet(out _tiltShift))
            {
                Debug.LogError($"{nameof(PersistentWorldCamera)} : need TiltShift override on Volume Profile...");
                return;
            }
            
            SetBandFeather(0.0f);
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

        public void BlurTo(float normalizedTime)
        {
            StopTween();
            SetBandFeather(normalizedTime);
        }

        private IEnumerator TweenStrength(float from, float to, float duration, bool useUnscaledTime, Action onComplete)
        {
            float elapsed = 0.0f;
            while (elapsed < duration)
            {
                elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                SetBandFeather(Mathf.Lerp(from, to, elapsed / duration));
                
                yield return null;
            }
            
            SetBandFeather(to);
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

        private void SetBandFeather(float normalizedTime)
        {
            var lerpBandFeather = BandFeather.Lerp(neutralBandFeather, targetBandFeather, normalizedTime);

            _tiltShift.bandHalfWidth.value = lerpBandFeather.bandHalfWidth;
            _tiltShift.feather.value = lerpBandFeather.feather;
        }
    }
}