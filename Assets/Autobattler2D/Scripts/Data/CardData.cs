using System;
using UnityEngine;

[Serializable]
public class CardData
{
    [SerializeField] 
    private int _id;
    public int ID => _id;
    
    [SerializeField] 
    private int _power;
    public int Power => _power;
    
    [SerializeField] 
    private Material _material;
    public Material Material => _material;
    
    [SerializeField] 
    private Sprite _sprite;
    public Sprite Sprite => _sprite;
    
    [SerializeField] 
    private string _name;
    public string Name => _name;
}
