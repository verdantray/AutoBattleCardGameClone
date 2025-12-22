using System;
using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
using ProjectABC.Core;
using ProjectABC.Utils;
using TMPro;
using UnityEngine;

namespace ProjectABC.Engine.UI
{
    public sealed class MatchPositionAnnounceUI : UIElement
    {
        [Header("Components")]
        [SerializeField] private TextMeshProUGUI txtMatchPosition;
        [Header("Flip Values")]
        [SerializeField] private ScaledTime flipDuration;
        [SerializeField] private ScaledTime flipDelay;
        [Header("Fade Values")]
        [SerializeField] private Vector2 fadePosition;
        [SerializeField] private ScaledTime fadeDuration;
        [SerializeField] private ScaledTime fadeDelay;

        public static async Task ShowMatchPositionAsync(MatchPosition ownPlayerPosition, CancellationToken token)
        {
            try
            {
                var matchPositionAnnounceUI = UIManager.Instance.OpenUI<MatchPositionAnnounceUI>();
                matchPositionAnnounceUI.SetOwnPlayerPosition(ownPlayerPosition);

                await matchPositionAnnounceUI.FlipAsync(token);
                await matchPositionAnnounceUI.FadeAsync(token);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                
            }
            finally
            {
                UIManager.Instance.CloseUI<MatchPositionAnnounceUI>();
            }
        }

        private void SetOwnPlayerPosition(MatchPosition ownPlayerPosition)
        {
            txtMatchPosition.text = ownPlayerPosition.ToString();
            
            txtMatchPosition.rectTransform.rotation = Quaternion.Euler(Vector3.up * 90.0f);
            txtMatchPosition.rectTransform.anchoredPosition = Vector2.zero;
            txtMatchPosition.color = Color.white;
        }

        private async Task FlipAsync(CancellationToken token = default)
        {
            var flipTween = DOTween
                .To(
                    () => txtMatchPosition.rectTransform.eulerAngles,
                    flipPosition => txtMatchPosition.rectTransform.eulerAngles = flipPosition,
                    Vector3.zero,
                    flipDuration
                )
                .SetUpdate(UpdateType.Manual);
                
            flipTween.Pause();

            try
            {
                await flipDelay.WaitScaledTimeAsync(token);

                flipTween.Play();

                while (flipTween.IsActiveAndPlaying())
                {
                    token.ThrowIfCancellationRequested();
                    await Task.Yield();

                    float timescale = MatchSimulationTimeScaler.Timescale;
                    if (timescale <= 0.0f)
                    {
                        continue;
                    }

                    float delta = Time.deltaTime * timescale;
                    flipTween.ManualUpdate(delta, delta);
                }
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {

            }
            finally
            {
                flipTween.Kill(true);
            }
        }

        private async Task FadeAsync(CancellationToken token = default)
        {
            var moveTween = DOTween
                .To(
                    () => txtMatchPosition.rectTransform.anchoredPosition,
                    pos => txtMatchPosition.rectTransform.anchoredPosition = pos,
                    fadePosition,
                    fadeDuration
                )
                .SetUpdate(UpdateType.Manual);
            moveTween.Pause();
            
            var fadeTween = DOTween
                .To(
                    () => txtMatchPosition.color,
                    col => txtMatchPosition.color = col,
                    Color.clear,
                    fadeDuration
                )
                .SetUpdate(UpdateType.Manual);
            fadeTween.Pause();

            try
            {
                await fadeDelay.WaitScaledTimeAsync(token);

                moveTween.Play();
                fadeTween.Play();

                while (true)
                {
                    token.ThrowIfCancellationRequested();
                    await Task.Yield();

                    float timescale = MatchSimulationTimeScaler.Timescale;
                    if (timescale <= 0.0f)
                    {
                        continue;
                    }

                    float delta = Time.deltaTime * timescale;

                    if (moveTween.IsActiveAndPlaying())
                    {
                        moveTween.ManualUpdate(delta, delta);
                    }

                    if (fadeTween.IsActiveAndPlaying())
                    {
                        fadeTween.ManualUpdate(delta, delta);
                    }

                    if (!moveTween.IsActiveAndPlaying() && !fadeTween.IsActiveAndPlaying())
                    {
                        break;
                    }
                }
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {

            }
            finally
            {
                moveTween.Kill(true);
                fadeTween.Kill(true);
            }
        }

        public override void Refresh()
        {
            
        }
    }
}
