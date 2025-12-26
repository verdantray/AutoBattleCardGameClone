using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
using ProjectABC.Utils;
using TMPro;
using UnityEngine;

namespace ProjectABC.Engine.UI
{
    public sealed class MatchLogUI : UIElement
    {
        [Header("Components")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform logBody;
        [SerializeField] private TextMeshProUGUI txtMessage;
        [Header("Values")]
        [SerializeField] private ScaledTime duration;
        [SerializeField] private ScaledTime remainDelay;

        private CancellationTokenSource _cts = null;

        public static void SendLog(string logMessage)
        {
            var matchLogUI = UIManager.Instance.OpenUI<MatchLogUI>();
            
            matchLogUI.ReceiveLogMessage(logMessage);
            matchLogUI.Refresh();
        }
        
        public override void Refresh()
        {
            CancelShowMatchLog();
            
            canvasGroup.alpha = 0.0f;
            logBody.anchoredPosition = Vector2.up * 20.0f;
            _cts = new CancellationTokenSource();
            
            ShowMatchLogAsync(_cts.Token).Forget();
        }

        private void ReceiveLogMessage(string logMessage)
        {
            txtMessage.text = logMessage;
        }

        private void CancelShowMatchLog()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
            }
            
            _cts = null;
        }
            
        private async Task ShowMatchLogAsync(CancellationToken token)
        {
            var fadeTween = DOTween
                .To(
                    () => canvasGroup.alpha,
                    alpha => canvasGroup.alpha = alpha,
                    1.0f,
                    duration
                );

            var moveTween = DOTween
                .To(
                    () => logBody.anchoredPosition,
                    pos => logBody.anchoredPosition = pos,
                    Vector2.up * 100.0f,
                    duration
                );

            await Task.WhenAll(
                MatchSimulationTimeScaler.PlayTweenWhileScaledTimeAsync(fadeTween, token),
                MatchSimulationTimeScaler.PlayTweenWhileScaledTimeAsync(moveTween, token)
            );

            await remainDelay.WaitScaledTimeAsync(token);

            UIManager.Instance.CloseUI<MatchLogUI>();
        }
    }
}
