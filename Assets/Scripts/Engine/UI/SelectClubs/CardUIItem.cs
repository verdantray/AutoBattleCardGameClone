using ProjectABC.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectABC.Engine.UI
{
    public class CardUIItem : ScrollListItem<CardSnapshot>
    {
        [SerializeField] private TextMeshProUGUI txtPower;
        [SerializeField] private TextMeshProUGUI txtName;
        [SerializeField] private TextMeshProUGUI txtTitle;
        [SerializeField] private TextMeshProUGUI txtDesc;
        [SerializeField] private GameObject descAreaLayer;
        [SerializeField] private Button btnDetail;

        private void Awake()
        {
            btnDetail.onClick.AddListener(OnClickDetail);
        }

        private void OnDestroy()
        {
            btnDetail.onClick.RemoveAllListeners();
        }

        private void OnClickDetail()
        {
            
        }

        public override void ApplyData(CardSnapshot data)
        {
            base.ApplyData(data);
            
            txtPower.text = $"{data.Power}";
            txtName.text = data.Name;
            txtTitle.text = data.Title;
            txtDesc.text = data.Description;
            
            bool isDescEmpty = string.IsNullOrEmpty(data.Description);
            descAreaLayer.SetActive(!isDescEmpty);
        }
    }
}
