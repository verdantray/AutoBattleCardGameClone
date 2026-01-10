using System;
using System.Threading.Tasks;
using DG.Tweening;
using ProjectABC.Core;
using ProjectABC.Utils;
using TMPro;
using UnityEngine;


namespace ProjectABC.Engine.UI
{
    public class GainWinPointsAnnounceUI : UIElement
    {
        [Header("Components")]
        [SerializeField] private TextMeshProUGUI txtPlayerName;
        [SerializeField] private TextMeshProUGUI txtPlayerScore;
        [SerializeField] private Animator animator;
        [Header("Values")]
        [SerializeField] private float appearDelay;
        [SerializeField] private float increaseDuration;
        [SerializeField] private float remainDelay;
        
        private readonly int _playAnimHash = Animator.StringToHash("Appear");

        public static void ShowGainWinPoints(GainWinPointsEvent data)
        {
            var announceUI = UIManager.Instance.OpenUI<GainWinPointsAnnounceUI>();
            int from = data.TotalPoints - data.GainPoints;
            
            announceUI.SetPlayerInfo(data.Player);
            announceUI.SetPlayerScore(from);
            announceUI.ShowIncreaseAsync(from, data.TotalPoints).Forget();
        }

        private void SetPlayerInfo(IPlayer player)
        {
            txtPlayerName.text = player.Name;
        }

        private void SetPlayerScore(int score)
        {
            txtPlayerScore.text = $"{score:D}";
        }

        private async Task ShowIncreaseAsync(int from, int to)
        {
            animator.Play(_playAnimHash);
            await Task.Delay(TimeSpan.FromSeconds(appearDelay));

            var scoreIncreaseTween = DOTween.To(
                () => from,
                SetPlayerScore,
                to,
                increaseDuration
            );

            scoreIncreaseTween.Play();
            await Task.Delay(TimeSpan.FromSeconds(increaseDuration + remainDelay));

            UIManager.Instance.CloseUI<GainWinPointsAnnounceUI>();
        }
        
        public override void Refresh()
        {
            
        }
    }
}
