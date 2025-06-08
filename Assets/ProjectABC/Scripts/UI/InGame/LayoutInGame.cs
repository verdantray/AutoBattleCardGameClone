using System.Collections;
using TMPro;
using UnityEngine;

namespace ProjectABC.Core
{
    public class LayoutInGame : LayoutBase
    {
        private const float TimeToShowSplash = 0.5f;
        private const float TimeToWaitSplash = 1f;
        private const float TimeToHideSplash = 0.5f;
        
        public TextMeshProUGUI _textScore;
        
        public CanvasGroup _splashWin;
        public CanvasGroup _splashLose;

        private int _debugWinCount;
        private int _debugLoseCount;
        
        public void OnEndRound(PlayerType playerType)
        {
            switch (playerType)
            {
                case PlayerType.Player:
                    _debugWinCount++;
                    StartCoroutine(ShowSplash(_splashWin));
                    break;

                case PlayerType.Enemy:
                    _debugLoseCount++;
                    StartCoroutine(ShowSplash(_splashLose));
                    break;
            }

            _textScore.text = $"{_debugWinCount} : {_debugLoseCount}";
        }

        public IEnumerator ShowSplash(CanvasGroup canvasGroup)
        {
            for (var t = 0f; t <= 1f; t += Time.deltaTime / TimeToShowSplash)
            {
                canvasGroup.alpha = t;
                yield return null;
            }
            canvasGroup.alpha = 1f;

            yield return new WaitForSeconds(TimeToWaitSplash);
            
            for (var t = 0f; t <= 1f; t += Time.deltaTime / TimeToHideSplash)
            {
                canvasGroup.alpha = 1f - t;
                yield return null;
            }
            canvasGroup.alpha = 0f;
        }
    }
}