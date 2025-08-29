using System;
using ProjectABC.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectABC.InGame
{
    public class ButtonCardSelect : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _name;
        [SerializeField] private TextMeshProUGUI _power;
        [SerializeField] private Image _icon;
        [SerializeField] private Image _shadow;
        [SerializeField] private Button _btnSelect;
        
        private int _index;
        private Card _card;
        
        public Action<int> OnSelectCard;

        private void Awake()
        {
            _btnSelect.onClick.AddListener(OnSelectClicked);
        }

        public void SetIndex(int index)
        {
            _index = index;
        }
        public void SetCard(Card card)
        {
            _card = card;
            
            _name.text = _card.Name;
            _power.text = _card.Power.ToString();
            // _icon.sprite = card.imagePath;
        }
        public Card GetCard() { return _card; }

        public void SetSelected(bool state)
        {
            _shadow.enabled = !state;
        }
        
        private void OnSelectClicked()
        {
            OnSelectCard.Invoke(_index);
        }
    }
}