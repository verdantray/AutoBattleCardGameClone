using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        private CardSpawnArgs _spawnArgs;
        
        public override void OnSpawned(CardSpawnArgs args)
        {
            ApplyArgs(args);
            transform.position = args.Position;
            transform.rotation = args.Rotation;
        }

        public override void OnDespawned()
        {
            
        }

        protected override void ApplyArgs(CardSpawnArgs args)
        {
            _spawnArgs = args;
            if (_spawnArgs?.CardReference?.CardData == null)
            {
                return;
            }

            CardData cardData = _spawnArgs.CardReference.CardData;
            
            string gradeSpriteName = $"grade_{cardData.gradeType.GradeTypeToOrdinalString()}";
            
            // gradeRenderer.sprite = GlobalAssetBinder.Instance.AtlasBinder.GetCardSprite(gradeSpriteName);

            int totalPower = cardData.basePower;
            totalPower += args.CardReference.Buffs.Sum(buff => buff.AdditivePower);
            
            powerText.text = totalPower.ToString(CultureInfo.InvariantCulture);
            rollText.text = LocalizationHelper.Instance.Localize(cardData.titleKey);
            nameText.text = LocalizationHelper.Instance.Localize(cardData.nameKey);
        }

        private void ChangeMainTextureOfMesh(MeshRenderer meshRenderer, Sprite sprite)
        {
            var mat = meshRenderer.materials[MATERIAL_INDEX_INCLUDING_MAIN_TEX];
            mat.SetTexture(MAIN_TEX_PROPERTY, sprite.texture);
        }

        public async Task MoveFollowingSplineAsync(SplineContainer splineContainer, ScaledTime duration, ScaledTime delay, CancellationToken token = default)
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

        public void MoveToTransform(Transform targetTransform)
        {
            transform.position = targetTransform.position;
            transform.rotation = targetTransform.rotation;
        }
    }
}
