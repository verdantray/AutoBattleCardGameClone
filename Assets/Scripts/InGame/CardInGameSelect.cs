using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectABC.InGame
{
    public class CardInGameSelect : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _name;
        [SerializeField] private TextMeshProUGUI _power;
        [SerializeField] private Image _icon;
        [SerializeField] private Image _shadow;
        [SerializeField] private Button _btnSelect;
        [SerializeField] private Button _btnReroll;
        
        private int _index;
        private CardDataOld _data;
        
        public Action<int> OnRerollCard;
        public Action<int> OnSelectCard;

        private void Awake()
        {
            _btnSelect.onClick.AddListener(OnSelectClicked);
            _btnReroll.onClick.AddListener(OnRerollClicked);
        }

        public void SetIndex(int index)
        {
            _index = index;
        }
        public void SetCard(CardDataOld card)
        {
            _data = card;

            _name.text = card.Name;
            _power.text = card.Power.ToString();
            _icon.sprite = card.Sprite;
        }
        public int GetCardID()
        {
            return _data.ID;
        }

        public void SetSelected(bool state)
        {
            _shadow.enabled = !state;
        }
        
        private void OnSelectClicked()
        {
            OnSelectCard.Invoke(_index);
        }
        private void OnRerollClicked()
        {
            OnRerollCard.Invoke(_index);
        }
    }
}