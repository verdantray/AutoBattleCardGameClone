using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ProjectABC.Core;
using TMPro;
using UnityEngine;

namespace ProjectABC.Engine.UI
{
    public sealed class MatchPlayersAnnounceUI : UIElement
    {
        [SerializeField] private TextMeshProUGUI txtOwnPlayerName;
        [SerializeField] private TextMeshProUGUI txtOtherPlayerName;
        [SerializeField] private float duration;

        private IPlayer[] _participants = null;

        public static async Task ShowMatchPlayersAsync(IPlayer[] participants, CancellationToken token = default)
        {
            var matchPlayersAnnounceUI = UIManager.Instance.OpenUI<MatchPlayersAnnounceUI>();
            matchPlayersAnnounceUI.SetParticipants(participants);
            matchPlayersAnnounceUI.Refresh();

            await Task.Delay(TimeSpan.FromSeconds(matchPlayersAnnounceUI.duration), token);
            
            UIManager.Instance.CloseUI<MatchPlayersAnnounceUI>();
        }

        public void SetParticipants(IPlayer[] participants)
        {
            _participants = participants;
        }

        public override void OnClose()
        {
            _participants = null;
        }

        public override void Refresh()
        {
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
    }
}
