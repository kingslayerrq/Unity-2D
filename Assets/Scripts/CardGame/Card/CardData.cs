using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string id;

    [Header("Base Card Info")] 
    [SerializeField] private CardClass cardClass;
    [SerializeField] private string cardName;
    [TextArea]
    [SerializeField] private string cardDescription;
    [SerializeField] private int cardPrice;
    [SerializeField] private int cardCost;
    [SerializeField] private CardRarity cardRarity;
    [SerializeField] private int cardLevel;
    [SerializeField] private CardType cardType;
    [SerializeField] private Sprite cardSprite;
    [Tooltip("The primary target type, where you drop the card onto")]
    [SerializeField] private TargetType targetType;
    [SerializeField] private List<KeywordData> keywords;
    
    [Header("Upgraded Card Info")]
    [SerializeField] private string upgradedCardName;
    [TextArea]
    [SerializeField] private string upgradedCardDescription;
    [SerializeField] private int upgradedCardPrice;
    [SerializeField] private int upgradedCardCost;
    [SerializeField] private Sprite upgradedCardSprite;
    [SerializeField] private TargetType upgradedTargetType;
    [SerializeField] private List<KeywordData> upgradedKeywords;

    [Header("Rarity & Unlock Settings")] 
    [Tooltip("Controls how often the card appears in rewards. NOT USED!!!")]
    private int spawnWeight;
    [UnityEngine.Serialization.FormerlySerializedAs("isUnlocked")]
    [SerializeField] private bool isUnlockedByDefault = true;

    [Header("Effects")]
    [SerializeField] private List<CardEffectData> cardEffects;
    [SerializeField] private List<CardEffectData> upgradedCardEffects;
    
    // Getters
    public string ID => id;
    public CardClass CardClass => cardClass;
    public string CardName => cardName;
    public string CardDescription => cardDescription;
    public int CardPrice => cardPrice;
    public int CardCost => cardCost;
    public CardRarity CardRarity => cardRarity;
    public int SpawnWeight => spawnWeight;
    public int CardLevel => cardLevel;
    public CardType CardType => cardType;
    public Sprite CardSprite => cardSprite;
    public TargetType TargetType => targetType;
    public IReadOnlyList<KeywordData> Keywords => keywords;
    public IReadOnlyList<CardEffectData> CardEffects => cardEffects;

    public string UpgradedCardName => upgradedCardName;
    public string UpgradedCardDescription => upgradedCardDescription;
    public int UpgradedCardPrice => upgradedCardPrice;
    public int UpgradedCardCost => upgradedCardCost;
    public Sprite UpgradedCardSprite => upgradedCardSprite;
    public TargetType UpgradedTargetType => upgradedTargetType;
    public IReadOnlyList<KeywordData> UpgradedKeywords => upgradedKeywords;
    public IReadOnlyList<CardEffectData> UpgradedCardEffects => upgradedCardEffects;
    
    public bool IsUnlockedByDefault => isUnlockedByDefault;

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Auto-assign unique GUID
        if (string.IsNullOrEmpty(id))
        {
            string path = UnityEditor.AssetDatabase.GetAssetPath(this);
            id = UnityEditor.AssetDatabase.AssetPathToGUID(path);
            UnityEditor.EditorUtility.SetDirty(this);
        }

        if (!string.IsNullOrWhiteSpace(cardName) && cardName == this.name) return;
        cardName = this.name.Replace("_", " ");
        upgradedCardName = (this.name + "+").Replace("_", " ");
    }
#endif
}

public enum TargetType
{
    Self,
    Enemy,
    AllEnemy,
    Player,
    AllPlayer,
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
    Defect,
    None
}


