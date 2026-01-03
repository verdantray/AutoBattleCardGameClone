using System.Linq;
using ProjectABC.Core;
using ProjectABC.Data;
using TMPro;
using UnityEngine;

namespace ProjectABC.Engine.UI
{
    public class CardUIItem : ScrollListItem<CardReference>
    {
        [SerializeField] private TextMeshProUGUI txtPower;
        [SerializeField] private TextMeshProUGUI txtGrade;
        [SerializeField] private TextMeshProUGUI txtName;
        [SerializeField] private TextMeshProUGUI txtTitle;
        [SerializeField] private TextMeshProUGUI txtDesc;
        [SerializeField] private GameObject descAreaLayer;

        public override void ApplyData(CardReference data)
        {
            base.ApplyData(data);
            CardData cardData = data.CardData;

            int totalPower = cardData.basePower;
            totalPower += data.Buffs.Sum(buff => buff.AdditivePower);
            
            txtPower.text = $"{totalPower}";
            txtGrade.text = LocalizationHelper.Instance.Localize(cardData.gradeType.GetLocalizationKey());
            txtName.text = LocalizationHelper.Instance.Localize(cardData.nameKey);
            txtTitle.text = LocalizationHelper.Instance.Localize(cardData.titleKey);

            bool isCardEffectExists = !string.IsNullOrEmpty(cardData.descKey);
            txtDesc.text = isCardEffectExists ? LocalizationHelper.Instance.Localize(cardData.descKey) : string.Empty;
            
            descAreaLayer.SetActive(isCardEffectExists);
        }
    }
}
