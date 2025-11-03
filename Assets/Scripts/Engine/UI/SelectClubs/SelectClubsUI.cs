using System;
using System.Collections;
using System.Collections.Generic;
using ProjectABC.Core;
using ProjectABC.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectABC.Engine.UI
{
    public sealed class SelectClubsUI : UIElement
    {
        [SerializeField] private SelectableClubListViewer listViewer;
        [SerializeField] private SelectedClubItem[] selectedClubItems;
        [SerializeField] private TextMeshProUGUI txtSelectedClubCount;
        [SerializeField] private Button btnConfirm;

        public ClubType SelectedClubFlag
        {
            get => _selectedClubFlag;
            private set
            {
                _selectedClubFlag = value;
                btnConfirm.interactable = IsAcceptableCondition;
            }
        }

        private ClubType _selectedClubFlag;
        private ClubType _fixedClubFlag;
        private ClubType _selectableClubFlag;

        private bool IsAcceptableCondition => SelectedClubFlag.HasFlag(_fixedClubFlag)
                                              && SelectedClubFlag.CountFlags() == GameConst.GameOption.SELECT_CLUB_TYPES_AMOUNT;

        private void Awake()
        {
            btnConfirm.onClick.AddListener(OnConfirm);
        }

        private void OnDestroy()
        {
            btnConfirm.onClick.RemoveAllListeners();
        }

        public void SelectClubsForDeckConstruction(ClubType fixedClubFlag, ClubType selectableClubFlag)
        {
            SelectedClubFlag |= fixedClubFlag;
            SelectedClubFlag |= selectableClubFlag;
            
            _fixedClubFlag = fixedClubFlag;
            _selectableClubFlag = selectableClubFlag;
            
            Refresh();
        }

        private void OnConfirm()
        {
            if (!listViewer.Initialized || !IsAcceptableCondition)
            {
                return;
            }
            
            UIManager.Instance.CloseUI<SelectClubsUI>();
        }

        public void ToggleClubFlag(ClubType club)
        {
            if (_fixedClubFlag.HasFlag(club) || !_selectableClubFlag.HasFlag(club))
            {
                return;
            }

            if (SelectedClubFlag.HasFlag(club))
            {
                SelectedClubFlag &= ~club;
            }
            else
            {
                SelectedClubFlag |= club;
            }
            
            Refresh();
        }
        
        public override void Refresh()
        {
            ShowList(new SelectableClubDataProvider(SelectedClubFlag, _fixedClubFlag, _selectableClubFlag));
            ShowCurrentSelectedClubs(SelectedClubFlag);
        }

        private void ShowList(SelectableClubDataProvider dataProvider)
        {
            listViewer.FetchData(dataProvider);
        }

        private void ShowCurrentSelectedClubs(ClubType selectedClubFlag)
        {
            int selectedClubCount = 0;
            
            foreach (var selectedClubItem in selectedClubItems)
            {
                bool isSelected = selectedClubFlag.HasFlag(selectedClubItem.targetClub);
                selectedClubItem.clubItem.SetActive(isSelected);

                selectedClubCount += isSelected ? 1 : 0;
            }

            txtSelectedClubCount.text = $"{selectedClubCount} / {GameConst.GameOption.SELECT_CLUB_TYPES_AMOUNT}";
            txtSelectedClubCount.color = IsAcceptableCondition ? Color.black : Color.red;
        }

        #region private classes for configure SelectClubsUI

        [Serializable]
        private class SelectedClubItem
        {
            public ClubType targetClub;
            public GameObject clubItem;
        }

        private class SelectableClubDataProvider : IReadOnlyCollection<SelectableClubData>
        {
            private readonly List<SelectableClubData> _dataList = new List<SelectableClubData>();
            
            public SelectableClubDataProvider(ClubType selectedClubFlag, ClubType fixedClubFlag, ClubType selectableClubFlag)
            {
                ClubType[] clubTypes = Enum.GetValues(typeof(ClubType)) as ClubType[];

                foreach (ClubType club in clubTypes!)
                {
                    bool isFixed = fixedClubFlag.HasFlag(club);
                    bool isSelectable = selectableClubFlag.HasFlag(club);
                    
                    if (!isFixed && !isSelectable)
                    {
                        continue;
                    }
                    
                    SelectableClubData data = new SelectableClubData
                    {
                        TargetClub = club,
                        IsFixed = isFixed,
                        IsSelected = selectedClubFlag.HasFlag(club),
                    };
                    
                    _dataList.Add(data);
                }
            }

            public IEnumerator<SelectableClubData> GetEnumerator() => _dataList.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => _dataList.GetEnumerator();

            public int Count => _dataList.Count;
        }

        #endregion
    }
}
