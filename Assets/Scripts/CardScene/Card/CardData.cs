using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Card Data")]
public class CardData: ScriptableObject
{
    [Header("Card Info")]
    [SerializeField] private CardClass cardClass;
    [SerializeField] private string cardName;
    [SerializeField] private string cardDescription;
    [SerializeField] private int cardPrice;
    [SerializeField] private int cardCost;
    [SerializeField] private CardRarity cardRarity;
    [SerializeField] private int cardLevel;
    [SerializeField] private CardType cardType;
    [SerializeField] private Sprite cardSprite;
    [SerializeField] private TargetType targetType;
    [SerializeField] private List<KeywordData> keywords;
    
    [Header("Effects")]
    [SerializeField] private List<CardEffectData> cardEffects;
    
    public CardClass CardClass => cardClass;
    
    public string CardName => cardName;

    public string CardDescription => cardDescription;

    public int CardPrice => cardPrice;

    public int CardCost => cardCost;
    
    public CardRarity CardRarity => cardRarity;

    public int CardLevel => cardLevel;

    public CardType CardType => cardType;

    public Sprite CardSprite => cardSprite;

    public TargetType TargetType => targetType;
    
    public List<KeywordData> Keywords => keywords;
    
    public IReadOnlyList<CardEffectData> CardEffects => cardEffects;

	
    
}

public enum TargetType
{
    Self,
    Enemy,
    AllEnemy,
    Ally,
    AllAlly,
    Any,
    None
}

// Attack, Skill, Power, etc...
public enum CardType
{
    Attack,
    Skill,
    Power,
    Status,
    Curse,
    Quest,
    None
}

public enum CardRarity
{
    Common,
    Uncommon,
    Rare,
}

public enum CardClass
{
    Ironclad,
    Silent,
    Regent,
    Necro,
    None
}


