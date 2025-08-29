using System;
using ProjectABC.Core;
using TMPro;
using UnityEngine;

namespace ProjectABC.InGame
{
    public class CardInGame : MonoBehaviour
    {
        private static readonly int HashTriggerFlip = Animator.StringToHash("Flip");

        [SerializeField] private TextMeshPro _name;
        [SerializeField] private TextMeshPro _power;
        [SerializeField] private MeshRenderer _meshRenderer;

        private Card _card;
        private CardInstance _cardInstance;

        private Animator _animator;

        public void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public void SetCard(Card card)
        {
            _card = card;

            _name.text = card.CardData.nameKey;
            _power.text = card.CardData.basePower.ToString();
            // _meshRenderer.material = card.CardData.imagePath;
        }

        public void SetCard(CardInstance card)
        {
            _cardInstance = card;

            _name.text = card.CardData.nameKey;
            _power.text = card.CardData.basePower.ToString();
            // _meshRenderer.material = card.CardData.imagePath;
        }

        public Card GetCard()
        {
            return _card;
        }

        public CardInstance GetCardInstance()
        {
            return _cardInstance;
        }

        public void FlipCard()
        {
            _animator.SetTrigger(HashTriggerFlip);
        }
    }
}