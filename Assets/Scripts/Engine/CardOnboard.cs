using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
using ProjectABC.Core;
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

        private CardReference _cardReference;
        
        public override void OnSpawned(CardSpawnArgs args)
        {
            MoveToTarget(args.Position, args.Angles);
            ApplyReference(args.CardReference);
        }

        public override void OnDespawned()
        {
            
        }

        public override void ApplyReference(CardReference reference)
        {
            _cardReference = reference;
            if (_cardReference == null)
            {
                return;
            }

            CardData cardData = _cardReference.CardData;
            
            string gradeSpriteName = $"grade_{cardData.gradeType.GradeTypeToOrdinalString()}";
            
            // gradeRenderer.sprite = GlobalAssetBinder.Instance.AtlasBinder.GetCardSprite(gradeSpriteName);

            int totalPower = cardData.basePower;
            totalPower += _cardReference.Buffs.Sum(buff => buff.AdditivePower);

            powerText.text = $"{totalPower:D}";

            if (totalPower > cardData.basePower)
            {
                powerText.color = Color.green;
            }
            else if (totalPower < cardData.basePower)
            {
                powerText.color = Color.red;
            }
            else
            {
                powerText.color = Color.black;
            }
            
            rollText.text = LocalizationHelper.Instance.Localize(cardData.titleKey);
            nameText.text = LocalizationHelper.Instance.Localize(cardData.nameKey);
        }

        private void ChangeMainTextureOfMesh(MeshRenderer meshRenderer, Sprite sprite)
        {
            var mat = meshRenderer.materials[MATERIAL_INDEX_INCLUDING_MAIN_TEX];
            mat.SetTexture(MAIN_TEX_PROPERTY, sprite.texture);
        }

        public async Task MoveFollowingSplineAsync(SplineContainer splineContainer, ScaledTime delay, ScaledTime duration, CancellationToken token)
        {
            await delay.WaitScaledTimeAsync(token);
            
            float scaledDuration = duration;
            if (scaledDuration < 0.0f)
            {
                ApplySplineAnimateSettings(splineContainer);
                splineAnimate.NormalizedTime = 1.0f;
                splineAnimate.Update();
                splineAnimate.Pause();
                return;
            }
            
            ApplySplineAnimateSettings(splineContainer);
            
            splineAnimate.Pause();
            splineAnimate.NormalizedTime = 0.0f;
            splineAnimate.Update();

            float t = 0.0f;

            try
            {
                while (t < 1.0f)
                {
                    token.ThrowIfCancellationRequested();
                    await Task.Yield();
                    
                    float timescale = MatchSimulationTimeScaler.Timescale;
                    if (timescale <= 0.0f)
                    {
                        continue;
                    }
                    
                    float delta = Time.deltaTime * timescale;

                    t += delta / scaledDuration;
                    t = Mathf.Min(1.0f, t);

                    float easedT = ApplyEasing(splineAnimate.Easing, t);

                    splineAnimate.NormalizedTime = easedT;
                    splineAnimate.Update();
                }
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                
            }
        }

        private void ApplySplineAnimateSettings(SplineContainer splineContainer)
        {
            splineAnimate.Container = splineContainer;
            splineAnimate.Duration = 1.0f;                  // actual animation time will controlled by normalized time
            splineAnimate.StartOffset = 0.0f;
            
            splineAnimate.AnimationMethod = SplineAnimate.Method.Time;
            splineAnimate.Loop = SplineAnimate.LoopMode.Once;
            splineAnimate.ObjectUpAxis = SplineComponent.AlignAxis.YAxis;
            splineAnimate.ObjectForwardAxis = SplineComponent.AlignAxis.ZAxis;
            splineAnimate.Alignment = SplineAnimate.AlignmentMode.None;
        }

        private float ApplyEasing(SplineAnimate.EasingMode mode, float t)
        {
            t = Mathf.Clamp01(t);

            return mode switch
            {
                SplineAnimate.EasingMode.None => t,
                SplineAnimate.EasingMode.EaseIn => t * t,
                SplineAnimate.EasingMode.EaseOut => 1.0f - ((1.0f - t) * (1.0f - t)),
                SplineAnimate.EasingMode.EaseInOut => t * t * (3.0f - (2.0f * t)),
                _ => t,
            };
        }

        public Task MoveToTargetAsync(Transform targetTransform, ScaledTime duration, CancellationToken token)
        {
            return MoveToTargetAsync(targetTransform.position, targetTransform.eulerAngles, duration, token);
        }

        public async Task MoveToTargetAsync(Vector3 position, Vector3 eulerAngles, ScaledTime duration, CancellationToken token)
        {
            var posTween = DOTween
                .To(
                    () => transform.position,
                    pos => transform.position = pos,
                    position,
                    duration
                );

            var rotTween = DOTween
                .To(
                    () => transform.eulerAngles,
                    angle => transform.eulerAngles = angle,
                    eulerAngles,
                    duration
                );

            var moveTask = Task.WhenAll(
                MatchSimulationTimeScaler.PlayTweenWhileScaledTimeAsync(posTween, token),
                MatchSimulationTimeScaler.PlayTweenWhileScaledTimeAsync(rotTween, token)
            );
            
            await moveTask;
        }

        public void MoveToTarget(Transform targetTransform)
        {
            MoveToTarget(targetTransform.position, targetTransform.eulerAngles);
        }

        public void MoveToTarget(Vector3 position, Vector3 eulerAngles)
        {
            transform.position = position;
            transform.eulerAngles = eulerAngles;
        }
    }
}
