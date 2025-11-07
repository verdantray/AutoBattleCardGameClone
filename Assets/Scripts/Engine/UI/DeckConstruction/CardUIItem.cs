using ProjectABC.Core;
using ProjectABC.Data;
using TMPro;
using UnityEngine;

namespace ProjectABC.Engine.UI
{
    public class CardUIItem : ScrollListItem<CardSnapshot>
    {
        [SerializeField] private TextMeshProUGUI txtPower;
        [SerializeField] private TextMeshProUGUI txtGrade;
        [SerializeField] private TextMeshProUGUI txtName;
        [SerializeField] private TextMeshProUGUI txtTitle;
        [SerializeField] private TextMeshProUGUI txtDesc;
        [SerializeField] private GameObject descAreaLayer;

        public override void ApplyData(CardSnapshot data)
        {
            base.ApplyData(data);
            
            txtPower.text = $"{data.Power}";
            txtGrade.text = LocalizationHelper.Instance.Localize(data.GradeType.GetLocalizationKey());
            txtName.text = data.Name;
            txtTitle.text = data.Title;
            txtDesc.text = data.Description;
            
            bool isDescEmpty = string.IsNullOrEmpty(data.Description);
            descAreaLayer.SetActive(!isDescEmpty);
        }
    }
}
