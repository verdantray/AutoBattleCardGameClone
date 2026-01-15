using System;
using System.Collections.Generic;
using System.Linq;
using ProjectABC.Core;
using ProjectABC.Data;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectABC.Engine.UI
{
    public sealed class SelectRecruitCardPoolUI : UIElement
    {
        [SerializeField] private RecruitCardPoolToggle[] toggles;
        [SerializeField] private Button btnConfirm;
        
        public int FocusIndex { get; private set; }

        private void Awake()
        {
            btnConfirm.onClick.AddListener(OnConfirm);
        }

        private void OnDestroy()
        {
            btnConfirm.onClick.RemoveAllListeners();
        }

        public void SetRecruitCardPools(IReadOnlyList<Tuple<GradeType, int>> gradeAmountPairs)
        {
            for (int i = 0; i < toggles.Length; i++)
            {
                toggles[i].ClearToggle();
                if (gradeAmountPairs.Count <= i)
                {
                    continue;
                }
                
                toggles[i].ApplyGradeAmountPair(gradeAmountPairs[i], OnToggleValueChanged);
            }
            
            Refresh();
        }

        private void OnConfirm()
        {
            UIManager.Instance.CloseUI<SelectRecruitCardPoolUI>();
        }

        public override void OnOpen()
        {
            FocusIndex = 0;
        }

        public override void OnClose()
        {
            foreach (var toggle in toggles)
            {
                toggle.ClearToggle();
            }
        }

        public override void Refresh()
        {
            FocusIndex = Mathf.Max(0, Array.FindIndex(toggles, FindActiveToggle));
            btnConfirm.interactable = toggles.Any(FindActiveToggle);
        }

        private void OnToggleValueChanged(bool value)
        {
            if (toggles.Count(FindActiveToggle) > 1 && !value)
            {
                return;
            }
            
            Refresh();
        }

        private static bool FindActiveToggle(RecruitCardPoolToggle toggle)
        {
            return toggle.IsOn;
        }
    }
}
