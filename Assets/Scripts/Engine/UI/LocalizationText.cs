using System.Collections.Generic;
using ProjectABC.Core;
using TMPro;
using UnityEngine;

namespace ProjectABC.Engine.UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LocalizationText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI tmpText;
        [SerializeField] private string assignedLocalizationKey;

        private static readonly List<LocalizationText> ActiveInstances = new List<LocalizationText>();

        private void Reset()
        {
            tmpText = GetComponent<TextMeshProUGUI>();
        }
        
        private void OnEnable()
        {
            Refresh();
            ActiveInstances.Add(this);
        }

        private void OnDisable()
        {
            ActiveInstances.Remove(this);
        }

        private void OnDestroy()
        {
            ActiveInstances.Remove(this);
        }

        public void Refresh()
        {
            Refresh(assignedLocalizationKey);
        }

        public void Refresh(string localizationKey)
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            if (!LocalizationHelper.HasInstance)
            {
                return;
            }
            
            tmpText.text = LocalizationHelper.Instance.Localize(localizationKey);
            // Debug.Log($"Font : {tmpText.font.name} / {tmpText.font.fallbackFontAssetTable.Count}");
        }

        public static void RefreshAllActiveInstances()
        {
            foreach (var instance in ActiveInstances)
            {
                instance.Refresh();
            }
        }
    }
}
