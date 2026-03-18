using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Card Data")]
public class CardData: ScriptableObject
{
    [Header("Card Info")]
    [SerializeField] private string cardName;
    [SerializeField] private string cardDescription;
    [SerializeField] private int cardPrice;
    [SerializeField] private int cardCost;
    [SerializeField] private CardRarity cardRarity;
    [SerializeField] private int cardLevel;
    [SerializeField] private CardType cardType;
    [SerializeField] private Sprite cardSprite;
    [SerializeField] private TargetType targetType;
    
    [Header("Effects")]
    public List<CardEffectData> cardEffects;

    public string CardName => cardName;

    public string CardDescription => cardDescription;

    public int CardPrice => cardPrice;

    public int CardCost => cardCost;
    
    public CardRarity CardRarity => cardRarity;

    public int CardLevel => cardLevel;

    public CardType CardType => cardType;

    public Sprite CardSprite => cardSprite;

    public TargetType TargetType => targetType;

    public List<CardEffectData> CardEffects => cardEffects;
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
    Quest
}

public enum CardRarity
{
    Common,
    Uncommon,
    Rare,
}


