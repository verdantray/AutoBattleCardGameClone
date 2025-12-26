using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
using ProjectABC.Core;
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

                await matchPositionAnnounceUI.ShowAsync(token);
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

        private async Task ShowAsync(CancellationToken token)
        {
            await flipDelay.WaitScaledTimeAsync(token);
            
            var flipTween = DOTween
                .To(
                    () => txtMatchPosition.rectTransform.eulerAngles,
                    flipPosition => txtMatchPosition.rectTransform.eulerAngles = flipPosition,
                    Vector3.zero,
                    flipDuration
                );
            
            await MatchSimulationTimeScaler.PlayTweenWhileScaledTimeAsync(flipTween, token);
            
            await fadeDelay.WaitScaledTimeAsync(token);
            
            var moveTween = DOTween
                .To(
                    () => txtMatchPosition.rectTransform.anchoredPosition,
                    pos => txtMatchPosition.rectTransform.anchoredPosition = pos,
                    fadePosition,
                    fadeDuration
                );
            
            var moveTask = MatchSimulationTimeScaler.PlayTweenWhileScaledTimeAsync(moveTween, token);

            var fadeTween = DOTween
                .To(
                    () => txtMatchPosition.color,
                    col => txtMatchPosition.color = col,
                    Color.clear,
                    fadeDuration
                );
            
            var fadeTask = MatchSimulationTimeScaler.PlayTweenWhileScaledTimeAsync(fadeTween, token);
            
            await Task.WhenAll(moveTask, fadeTask);
        }

        public override void Refresh()
        {
            
        }
    }
}
