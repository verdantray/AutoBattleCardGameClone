using System;
using System.Threading.Tasks;
using ProjectABC.Utils;
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

        public static void AnnounceRound(int round)
        {
            var roundAnnounceUI = UIManager.Instance.OpenUI<RoundAnnounceUI>();
            roundAnnounceUI.txtRound.text = $"Round\n{round}";
            roundAnnounceUI.DelayClose().Forget();
        }

        public override void Refresh()
        {
            // do nothing
        }

        private async Task DelayClose()
        {
            animator.Play(_playAnimHash);
            await Task.Delay(TimeSpan.FromSeconds(duration));
            
            UIManager.Instance.CloseUI<RoundAnnounceUI>();
        }
    }
}
