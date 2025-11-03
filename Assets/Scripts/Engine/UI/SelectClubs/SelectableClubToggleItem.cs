using ProjectABC.Core;
using ProjectABC.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectABC.Engine.UI
{
    public sealed record SelectableClubData
    {
        public ClubType TargetClub;
        public bool IsSelected;
        public bool IsFixed;
        
        public void Deconstruct(out ClubType targetClub, out bool isSelected, out bool isFixed)
        {
            targetClub = TargetClub;
            isSelected = IsSelected;
            isFixed = IsFixed;
        }
    }
    
    public sealed class SelectableClubToggleItem : ScrollListItem<SelectableClubData>, IPointerClickHandler
    {
        [SerializeField] private TextMeshProUGUI txtClubName;
        [SerializeField] private TextMeshProUGUI txtClubDesc;
        [SerializeField] private Image imgClubSymbol;
        [SerializeField] private GameObject selectedLayer;
        [SerializeField] private GameObject fixedLayer;
        
        public override void ApplyData(SelectableClubData data)
        {
            base.ApplyData(data);
            var (club, isSelected, isFixed) = Data;
            
            txtClubName.text = LocalizationHelper.Instance.Localize(club.GetLocalizationKey());
            txtClubDesc.text = LocalizationHelper.Instance.Localize(club.GetLocalizationKey());
            selectedLayer.SetActive(isSelected);
            fixedLayer.SetActive(isFixed);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Data == null || Data.IsFixed)
            {
                return;
            }
            
            var selectUI = UIManager.Instance.GetUI<SelectClubsUI>();
            selectUI.ToggleClubFlag(Data.TargetClub);
        }
    }
}
