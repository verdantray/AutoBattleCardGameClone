using System;
using ProjectABC.Core;
using ProjectABC.Data;
using TMPro;
using UnityEngine;

namespace ProjectABC.Engine.UI
{
    public sealed class PlayerSelectedClubItem : ScrollListItem<DeckConstructionEvent>
    {
        [Serializable]
        private class SelectedClubTag
        {
            public ClubType targetClubType;
            public GameObject clubTag;
        }
        
        [SerializeField] private TextMeshProUGUI txtPlayerName;
        [SerializeField] private GameObject localPlayerTag;
        [SerializeField] private SelectedClubTag[] selectedClubTags;

        public override void ApplyData(DeckConstructionEvent data)
        {
            base.ApplyData(data);
            
            txtPlayerName.text = Data.Player.Name;
            localPlayerTag.SetActive(Data.Player.IsLocalPlayer);

            foreach (var selectedClubTag in selectedClubTags)
            {
                bool isSelected = Data.SelectedClubFlag.HasFlag(selectedClubTag.targetClubType);
                selectedClubTag.clubTag.SetActive(isSelected);
            }
        }
    }
}
