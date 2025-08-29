using System;
using ProjectABC.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectABC.InGame.UI
{
    public class ButtonCardPool : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _textCardPool;
        [SerializeField] private TextMeshProUGUI _textCardCount;
        [SerializeField] private Button _btnSelect;

        private int _index;
        
        public Action<int> OnSelectCardPool;

        private void Awake()
        {
            _btnSelect.onClick.AddListener(OnSelectClicked);
        }
        
        public void SetIndex(int index)
        {
            _index = index;
        }
        public void SetCardPool(GradeType gradeType, int count)
        {
            switch (gradeType)
            {
                case GradeType.First:
                    _textCardPool.text = "1학년";
                    break;
                case GradeType.Second:
                    _textCardPool.text = "2학년";
                    break;
                case GradeType.Third:
                    _textCardPool.text = "3학년";
                    break;
            }
            _textCardCount.text = $"x{count}";
        }
        
        private void OnSelectClicked()
        {
            OnSelectCardPool.Invoke(_index);
        }
    }
}