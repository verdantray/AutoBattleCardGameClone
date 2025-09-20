using System;
using ProjectABC.Core;
using TMPro;
using UnityEngine;

namespace ProjectABC.Engine
{
    public class CardInGame : MonoBehaviour
    {
        private static readonly int HashTriggerFlip = Animator.StringToHash("Flip");

        [SerializeField] private TextMeshPro _name;
        [SerializeField] private TextMeshPro _power;
        [SerializeField] private MeshRenderer _meshRenderer;

        private Card _card;
        private CardSnapshot _cardSnapshot;

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

            var materials = CardMaterialLoader.Instance.GetCardMaterials(card.CardData.imagePath);
            _meshRenderer.SetMaterials(materials);
        }

        public void SetCard(CardSnapshot card)
        {
            _cardSnapshot = card;

            _name.text = card.Name;
            _power.text = card.Power.ToString();
            
            var materials = CardMaterialLoader.Instance.GetCardMaterials(card.Card.CardData.imagePath);
            _meshRenderer.SetMaterials(materials);
        }

        public Card GetCard()
        {
            return _card;
        }

        public CardSnapshot GetCardInstance()
        {
            return _cardSnapshot;
        }

        public void FlipCard()
        {
            _animator.SetTrigger(HashTriggerFlip);
        }
    }
}