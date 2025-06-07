using System;
using TMPro;
using UnityEngine;

public class CardInGame : MonoBehaviour
{
    private static readonly int HashTriggerFlip = Animator.StringToHash("Flip");
    
    [SerializeField] private TextMeshPro _name;
    [SerializeField] private TextMeshPro _power;
    [SerializeField] private MeshRenderer _meshRenderer;

    private Card _card;
    
    private Animator _animator;

    public void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void SetCard(Card card)
    {
        _card = card;
        
        _name.text = card.Data.Name;
        _power.text = card.Data.Power.ToString();
        _meshRenderer.material = card.Data.Material;
    }

    public int GetCardID()
    {
        return _card.Data.ID;
    }

    public void FlipCard()
    {
        _animator.SetTrigger(HashTriggerFlip);
    }
}
