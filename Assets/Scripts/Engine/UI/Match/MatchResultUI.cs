using System;
using System.Threading.Tasks;
using ProjectABC.Core;
using ProjectABC.Utils;
using TMPro;
using UnityEngine;

namespace ProjectABC.Engine.UI
{
    public class MatchResultUI : UIElement
    {
        [Header("Components")]
        [SerializeField] private TextMeshProUGUI txtWinnerName;
        [SerializeField] private TextMeshProUGUI txtResultReason;
        [SerializeField] private Animator animator;
        [Header("Values")]
        [SerializeField] private float duration;
        
        private readonly int _playAnimHash = Animator.StringToHash("Appear");

        public static void ShowMatchResult(MatchFinishEvent finishEvent)
        {
            var matchResultUI = UIManager.Instance.OpenUI<MatchResultUI>();
            matchResultUI.SetResult(finishEvent.Winner, finishEvent.Reason);
            matchResultUI.DelayedClose().Forget();
        }
        
        public override void Refresh()
        {
            // do nothing
        }

        private void SetResult(IPlayer winner, MatchEndReason reason)
        {
            txtWinnerName.text = winner.Name;
            
            // TODO: use localization key
            txtResultReason.text = reason switch
            {
                MatchEndReason.EndByEmptyDeck => "Completely exhausted opponent's deck",
                MatchEndReason.EndByFullOfInfirmary => "All of opponent's infirmary slots have been filled",
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private async Task DelayedClose()
        {
            animator.Play(_playAnimHash);
            await Task.Delay(TimeSpan.FromSeconds(duration));

            UIManager.Instance.CloseUI<MatchResultUI>();
        }
    }
}
