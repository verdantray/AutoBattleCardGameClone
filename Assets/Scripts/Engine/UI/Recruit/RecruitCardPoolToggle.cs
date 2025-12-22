using System;
using ProjectABC.Core;
using ProjectABC.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectABC.Engine.UI
{
    public class RecruitCardPoolToggle : MonoBehaviour
    {
        [SerializeField] private GameObject[] amountLayers;
        [SerializeField] private TextMeshProUGUI[] txtGrades;
        [SerializeField] private TextMeshProUGUI txtGradeAmountPair;
        [SerializeField] private Toggle toggle;

        public bool IsOn => toggle.isOn;
        
        public void ApplyGradeAmountPair(Tuple<GradeType, int> gradeAmountPair, Action<bool> callback)
        {
            var (grade, amount) = gradeAmountPair;

            for (int i = 0; i < amountLayers.Length; i++)
            {
                // Debug.Log($"index : {i} / amount : {amount} / active : {i == (amount - 1)}");
                amountLayers[i].SetActive(i == (amount - 1));
            }
            
            string localizedGrade = LocalizationHelper.Instance.Localize(grade.GetLocalizationKey());

            foreach (var txtGrade in txtGrades)
            {
                txtGrade.text = localizedGrade;
            }

            txtGradeAmountPair.text = $"{localizedGrade} X {amount}";
            
            toggle.onValueChanged.AddListener(callback.Invoke);
            toggle.isOn = false;
            
            gameObject.SetActive(true);
        }

        public void ClearToggle()
        {
            toggle.onValueChanged.RemoveAllListeners();
            gameObject.SetActive(false);
        }
    }
}
