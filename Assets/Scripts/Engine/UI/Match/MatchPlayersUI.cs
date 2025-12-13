using System.Collections.Generic;
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

        public void ShowMatchPlayers(IEnumerable<MatchSideSnapshot> matchSideSnapshots)
        {
            foreach (var matchSideSnapshot in matchSideSnapshots)
            {
                if (ReferenceEquals(matchSideSnapshot.Player, Simulator.Model.player))
                {
                    txtOwnPlayerName.text = matchSideSnapshot.Player.Name;
                    txtOwnPlayerScore.text = $"{matchSideSnapshot.Score:D}";
                }
                else
                {
                    txtOtherPlayerName.text = matchSideSnapshot.Player.Name;
                    txtOtherPlayerScore.text = $"{matchSideSnapshot.Score:D}";
                }
            }
        }
        
        public override void Refresh()
        {
            
        }

        public void ShowChangingScore()
        {
            
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
