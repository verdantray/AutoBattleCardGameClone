using System;
using System.Collections;
using System.Globalization;
using ProjectABC.Data;
using TMPro;
using UnityEngine;
using UnityEngine.Splines;

namespace ProjectABC.Engine
{
    public sealed class CardOnboard : CardObject
    {
        private const int MATERIAL_INDEX_INCLUDING_MAIN_TEX = 0;
        private static readonly int MAIN_TEX_PROPERTY = Shader.PropertyToID("_MainTex");
        
        [SerializeField] private MeshRenderer studentRender;
        [SerializeField] private MeshRenderer sleeveRenderer;
        [SerializeField] private SpriteRenderer gradeRenderer;
        [SerializeField] private TMP_Text powerText;
        [SerializeField] private TMP_Text rollText;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private SplineAnimate splineAnimate;

        private CardSpawnArgs _spawnArgs;
        private Coroutine _moveLinearRoutine;
        
        public override void OnSpawned(CardSpawnArgs args)
        {
            ApplyArgs(args);
            transform.position = args.Position;
            transform.rotation = args.Rotation;
        }

        public override void OnDespawned()
        {
            CancelMovingLinear();
        }
        
        protected override void ApplyArgs(CardSpawnArgs args)
        {
            _spawnArgs = args;
            if (_spawnArgs == null)
            {
                return;
            }
            
            string gradeSpriteName = $"grade_{_spawnArgs.CardSnapshot.GradeType.GradeTypeToOrdinalString()}";
            
            gradeRenderer.sprite = GlobalAssetBinder.Instance.AtlasBinder.GetCardSprite(gradeSpriteName);
            powerText.text = _spawnArgs.CardSnapshot.Power.ToString(CultureInfo.InvariantCulture);
            rollText.text = _spawnArgs.CardSnapshot.Title;
            nameText.text = _spawnArgs.CardSnapshot.Name;
        }

        private void ChangeMainTextureOfMesh(MeshRenderer meshRenderer, Sprite sprite)
        {
            var mat = meshRenderer.materials[MATERIAL_INDEX_INCLUDING_MAIN_TEX];
            mat.SetTexture(MAIN_TEX_PROPERTY, sprite.texture);
        }

        public void MoveFollowingSpline(SplineContainer splineContainer, float duration, Action callback)
        {
            splineAnimate.Completed -= OnSplineAnimateCompleted;
            splineAnimate.Pause();
            
            splineAnimate.Container = splineContainer;
            splineAnimate.Duration = duration;

            splineAnimate.ObjectUpAxis = SplineComponent.AlignAxis.YAxis;
            splineAnimate.ObjectForwardAxis = SplineComponent.AlignAxis.ZAxis;
            splineAnimate.Alignment = SplineAnimate.AlignmentMode.SplineObject;
            splineAnimate.StartOffset = 0.0f;
            splineAnimate.AnimationMethod = SplineAnimate.Method.Time;
            splineAnimate.Easing = SplineAnimate.EasingMode.EaseInOut;
            splineAnimate.Loop = SplineAnimate.LoopMode.Once;

            splineAnimate.Completed += OnSplineAnimateCompleted;
            splineAnimate.Play();

            return;
            void OnSplineAnimateCompleted()
            {
                splineAnimate.Completed -= OnSplineAnimateCompleted;
                callback?.Invoke();
            }
        }

        public void MoveLinear(Transform destination, float duration, float delay = 0.0f, Action callback = null)
        {
            MoveLinear(destination.position, destination.rotation, duration, delay, callback);
        }

        public void MoveLinear(Vector3 position, Quaternion rotation, float duration, float delay = 0.0f, Action callback = null)
        {
            CancelMovingLinear();
            _moveLinearRoutine = StartCoroutine(Move(position, rotation, duration, delay, callback));
        }

        private void CancelMovingLinear()
        {
            if (_moveLinearRoutine != null)
            {
                StopCoroutine(_moveLinearRoutine);
            }
            
            _moveLinearRoutine = null;
        }

        private IEnumerator Move(Vector3 position, Quaternion rotation, float duration, float delay = 0.0f, Action callback = null)
        {
            if (delay > 0.0f)
            {
                yield return new WaitForSeconds(delay);
            }

            if (duration > 0.0f)
            {
                float elapsedTime = 0.0f;
            
                Vector3 startPosition = transform.position;
                Quaternion startRotation = transform.rotation;
            
                while (elapsedTime < duration)
                {
                    elapsedTime += Time.deltaTime;
                
                    Vector3 interpolatedPosition = Vector3.Lerp(startPosition, position, elapsedTime / duration);
                    Quaternion interpolatedRotation = Quaternion.Lerp(startRotation, rotation, elapsedTime / duration);

                    transform.position = interpolatedPosition;
                    transform.rotation = interpolatedRotation;
                
                    yield return null;
                }
            }

            transform.position = position;
            transform.rotation = rotation;
            
            callback?.Invoke();
            CancelMovingLinear();
        }
    }
}
