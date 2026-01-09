using ProjectABC.Core;
using ProjectABC.Data;
using TMPro;
using UnityEngine;

namespace ProjectABC.Engine.UI
{
    public class CardUIItem : ScrollListItem<CardData>
    {
        [SerializeField] private TextMeshProUGUI txtPower;
        [SerializeField] private TextMeshProUGUI txtGrade;
        [SerializeField] private TextMeshProUGUI txtName;
        [SerializeField] private TextMeshProUGUI txtTitle;
        [SerializeField] private TextMeshProUGUI txtDesc;
        [SerializeField] private GameObject descAreaLayer;

        public override void ApplyData(CardData data)
        {
            base.ApplyData(data);

            txtPower.text = $"{Data.basePower:D}";
            txtGrade.text = LocalizationHelper.Instance.Localize(Data.gradeType.GetLocalizationKey());
            txtName.text = LocalizationHelper.Instance.Localize(Data.nameKey);
            txtTitle.text = LocalizationHelper.Instance.Localize(Data.titleKey);

            bool isCardEffectExists = !string.IsNullOrEmpty(Data.descKey);
            txtDesc.text = isCardEffectExists ? LocalizationHelper.Instance.Localize(Data.descKey) : string.Empty;
            
            descAreaLayer.SetActive(isCardEffectExists);
        }
    }
}
