using System.Collections;
using TMPro;
using UnityEngine;

namespace ProjectABC.Engine.UI
{
    public sealed class RoundAnnounceUI : UIElement
    {
        [SerializeField] private TextMeshProUGUI txtRound;
        [SerializeField] private Animator animator;
        [SerializeField] private float duration;

        private readonly int _playAnimHash = Animator.StringToHash("Appear");

        private Coroutine _delayCloseRoutine;

        private void OnDisable()
        {
            StopRoutine();
        }

        public void AnnounceRound(int round)
        {
            txtRound.text = $"Round\n{round}";
            animator.Play(_playAnimHash);
            
            StopRoutine();
            _delayCloseRoutine = StartCoroutine(DelayClose(duration));
        }

        public override void Refresh()
        {
            // do nothing
        }

        private void StopRoutine()
        {
            if (_delayCloseRoutine != null)
            {
                StopCoroutine(_delayCloseRoutine);
            }
            
            _delayCloseRoutine = null;
        }

        private IEnumerator DelayClose(float closeDelay)
        {
            yield return new WaitForSeconds(closeDelay);

            UIManager.Instance.CloseUI<RoundAnnounceUI>();
            _delayCloseRoutine = null;
        }
    }
}
