using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
using ProjectABC.Core;
using ProjectABC.Utils;
using TMPro;
using UnityEngine;

namespace ProjectABC.Engine.UI
{
    public sealed class MatchPlayersAnnounceUI : UIElement
    {
        [Header("Components")]
        [SerializeField] private TextMeshProUGUI txtOwnPlayerName;
        [SerializeField] private TextMeshProUGUI txtOtherPlayerName;
        [SerializeField] private RectTransform ownPlayerTag;
        [SerializeField] private RectTransform otherPlayerTag;
        [SerializeField] private CanvasGroup canvasGroup;
        
        [Header("Tween Values")]
        [SerializeField] private float moveDistance;
        [SerializeField] private ScaledTime duration;
        [SerializeField] private ScaledTime remainingDelay;
        
        private IPlayer[] _participants = null;

        public static async Task ShowMatchPlayersAsync(IPlayer[] participants, CancellationToken token = default)
        {
            try
            {
                var matchPlayersAnnounceUI = UIManager.Instance.OpenUI<MatchPlayersAnnounceUI>();
                matchPlayersAnnounceUI.SetParticipants(participants);
                matchPlayersAnnounceUI.Refresh();

                await matchPlayersAnnounceUI.ShowMatchPlayersAsync(token);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                
            }
            finally
            {
                UIManager.Instance.CloseUI<MatchPlayersAnnounceUI>();
            }
        }

        private void SetParticipants(IPlayer[] participants)
        {
            _participants = participants;
        }

        public override void OnClose()
        {
            _participants = null;
        }

        public override void Refresh()
        {
            ownPlayerTag.anchoredPosition = Vector2.up * moveDistance;
            otherPlayerTag.anchoredPosition = Vector2.down * moveDistance;
            
            if (_participants == null)
            {
                return;
            }
            
            IPlayer ownPlayer = _participants.FirstOrDefault(player => ReferenceEquals(player, Simulator.Model.player));
            IPlayer otherPlayer = _participants.FirstOrDefault(player => !ReferenceEquals(player, Simulator.Model.player));

            if (ownPlayer == null || otherPlayer == null)
            {
                throw new ArgumentException($"unknown players.... {string.Join(", ", _participants.Select(player => player.Name))}");
            }

            txtOwnPlayerName.text = ownPlayer.Name;
            txtOtherPlayerName.text = otherPlayer.Name;
        }

        private async Task ShowMatchPlayersAsync(CancellationToken token = default)
        {
            var ownTagTween = DOTween
                .To(
                    () => ownPlayerTag.anchoredPosition,
                    pos => ownPlayerTag.anchoredPosition = pos,
                    Vector2.zero,
                    duration
                );
            var ownTagTask = MatchSimulationTimeScaler.PlayTweenWhileScaledTimeAsync(ownTagTween, token);

            var otherTagTween = DOTween
                .To(
                    () => otherPlayerTag.anchoredPosition,
                    pos => otherPlayerTag.anchoredPosition = pos,
                    Vector2.zero,
                    duration
                );
            var otherTagTask = MatchSimulationTimeScaler.PlayTweenWhileScaledTimeAsync(otherTagTween, token);

            var alphaTween = DOTween
                .To(
                    () => canvasGroup.alpha,
                    alpha => canvasGroup.alpha = alpha,
                    1.0f,
                    duration
                );
            var alphaTask = MatchSimulationTimeScaler.PlayTweenWhileScaledTimeAsync(alphaTween, token);
            
            await Task.WhenAll(ownTagTask, otherTagTask, alphaTask, otherTagTask);
            await remainingDelay.WaitScaledTimeAsync(token);
        }
    }
}
