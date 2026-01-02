using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
using ProjectABC.Core;
using TMPro;
using UnityEngine;

namespace ProjectABC.Engine.UI
{
    public sealed class MatchPlayersUI : UIElement
    {
        [Header("Components")]
        [SerializeField] private TextMeshProUGUI txtOwnPlayerName;
        [SerializeField] private TextMeshProUGUI txtOtherPlayerName;
        [SerializeField] private TextMeshProUGUI txtOwnPlayerScore;
        [SerializeField] private TextMeshProUGUI txtOtherPlayerScore;
        [Header("Tween Values")]
        [SerializeField] private ScaledTime scoreChangeDuration = 0.25f;

        public void ShowMatchPlayers(IEnumerable<MatchSideSnapshot> matchSideSnapshots)
        {
            foreach (var matchSideSnapshot in matchSideSnapshots)
            {
                SetPlayerNameAndScore(matchSideSnapshot.Player, matchSideSnapshot.Score);
            }
        }
        
        public override void Refresh()
        {
            
        }

        public Task GetShowChangingScoreTask(IPlayer player, int gainPoints, int totalPoints, CancellationToken token)
        {
            var scoreTween = DOTween.To(
                () => totalPoints - gainPoints,
                score => SetPlayerNameAndScore(player, score),
                totalPoints,
                scoreChangeDuration
            );

            return MatchSimulationTimeScaler.PlayTweenWhileScaledTimeAsync(scoreTween, token);
        }

        private void SetPlayerNameAndScore(IPlayer player, int score)
        {
            var nameText = player.IsLocalPlayer
                ? txtOwnPlayerName
                : txtOtherPlayerName;
            var scoreText = player.IsLocalPlayer
                ? txtOwnPlayerScore
                : txtOtherPlayerScore;

            nameText.text = player.Name;
            scoreText.text = $"{score:D}";
        }

        public override void OnClose()
        {
            txtOwnPlayerName.text = "";
            txtOtherPlayerName.text = "";
            txtOwnPlayerScore.text = "";
            txtOtherPlayerScore.text = "";
        }
    }
}
