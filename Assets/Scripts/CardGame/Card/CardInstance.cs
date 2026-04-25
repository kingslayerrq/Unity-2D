using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CardInstance
{
    
    public bool CanBePlayed => CheckForPlayability();
    [SerializeField] private CardData data;
    [NonSerialized] private BasePlayerUnitInstance currentOwner;
    [SerializeField] private int currentCost;
    [SerializeField] private string currentName;
    [SerializeField] private string currentDescription;
    [SerializeField] private int currentPrice;
    [SerializeField] private int currentLevel;
    [SerializeField] private bool currentIsUpgraded;
    [SerializeField] private bool currentIsEnchanted;
    [SerializeField] private Sprite currentSprite;
    [SerializeField] private TargetType currentTargetType;
    [SerializeField] private IReadOnlyList<CardEffectData> currentCardEffects;
    [SerializeField] private IReadOnlyList<KeywordData> currentKeywords;
    
    // public List<CardModifier> ActiveModifiers { get; private set; } = new List<CardModifier>();

    #region getters
    public CardData Data => data;
    public CardState CurrentCardState { get; set; }
    public BaseUnitInstance CurrentOwner => currentOwner;
    public int CurrentCost => currentCost;
    public string CurrentName => currentName;
    public string CurrentDescription => currentDescription;
    public CardType CurrentCardType => data.CardType;
    public int CurrentPrice => currentPrice;
    public int CurrentLevel => currentLevel;
    public bool CurrentIsEnchanted => currentIsEnchanted;
    public bool CurrentIsUpgraded => currentIsUpgraded;
    public Sprite CurrentSprite => currentSprite;
    public TargetType CurrentTargetType => currentTargetType;
    public  IReadOnlyList<CardEffectData> CurrentCardEffects => currentCardEffects;
    public IReadOnlyList<KeywordData> CurrentKeywords => currentKeywords;

    #endregion
    

    public CardInstance(CardData data)
    {
        // ref to base card data
        this.data = data;
        // instance stats
        this.CurrentCardState = CardState.Init;
        this.currentCost = data.CardCost;
        this.currentName = data.CardName;
        this.currentDescription = data.CardDescription;
        this.currentPrice = data.CardPrice;
        this.currentLevel = data.CardLevel;
        this.currentSprite = data.CardSprite;
        this.currentTargetType = data.TargetType;
        this.currentIsUpgraded = data.IsUpgraded;
        this.currentIsEnchanted = data.IsEnchanted;
        this.currentCardEffects = data.CardEffects;
        this.currentKeywords = data.Keywords;

        data.SetDiscovered(true);
    }
    
    public CardInstance(CardData data, BasePlayerUnitInstance owner)
    {
        // ref to base card data
        this.data = data;
        // instance stats
        this.CurrentCardState = CardState.Init;
        this.currentOwner = owner;
        this.currentCost = data.CardCost;
        this.currentName = data.CardName;
        this.currentDescription = data.CardDescription;
        this.currentPrice = data.CardPrice;
        this.currentLevel = data.CardLevel;
        this.currentSprite = data.CardSprite;
        this.currentTargetType = data.TargetType;
        this.currentIsUpgraded = data.IsUpgraded;
        this.currentIsEnchanted = data.IsEnchanted;
        this.currentCardEffects = data.CardEffects;
        this.currentKeywords = data.Keywords;

        data.SetDiscovered(true);
    }
    
    public CardInstance(CardData data, BasePlayerUnitInstance owner, bool upgrade, bool enchant)
    {
        // ref to base card data
        this.data = data;
        // instance stats
        this.CurrentCardState = CardState.Init;
        this.currentOwner = owner;
        this.currentCost = data.CardCost;
        this.currentName = data.CardName;
        this.currentDescription = data.CardDescription;
        this.currentPrice = data.CardPrice;
        this.currentLevel = data.CardLevel;
        this.currentSprite = data.CardSprite;
        this.currentTargetType = data.TargetType;
        this.currentIsUpgraded = upgrade;
        this.currentIsEnchanted = enchant;
        this.currentCardEffects = data.CardEffects;
        this.currentKeywords = data.Keywords;

        data.SetDiscovered(true);
    }
    
    public bool RequireTarget => Data.TargetType != TargetType.None;

    public bool IsExhaustOnPlay()
    {
        if (currentCardEffects == null) return false;
        foreach (var effectData in currentCardEffects)
        {
            if (effectData?.CardEffect is not ExhaustCardEffect) continue;
            // No config assigned → defaults to ThisCard
            if (effectData.CardEffectConfig is not ExhaustCardEffectConfig config) return true;
            return config.ExhaustMode == ExhaustMode.ThisCard;
        }
        return false;
    }

    public CardInstance Clone()
    {
        CardInstance clone = new CardInstance(this.data, this.currentOwner, this.currentIsUpgraded,
            this.currentIsEnchanted);
        return clone;
        
    }

    public void SetOwner(BasePlayerUnitInstance owner)
    {
        this.currentOwner = owner;
    }
    public void ModifyCost(int amount)
    {
        currentCost = amount;
    }

    public void AddCost(int amount)
    {
        currentCost += amount;
    }
    
    public void ReduceCost(int amount)
    {
        currentCost -= amount;
        if  (CurrentCost < 0)
        {
            currentCost = 0;
        }
    }

    public void Upgrade()
    {
        if (currentIsUpgraded) return;
        currentIsUpgraded = true;

        currentName = data.UpgradedCardName;
        currentDescription = data.UpgradedCardDescription;
        currentPrice = data.UpgradedCardPrice;
        currentCost = data.UpgradedCardCost;
        currentSprite = data.UpgradedCardSprite;
        currentCardEffects = data.UpgradedCardEffects;
        currentKeywords = data.UpgradedKeywords;
        currentTargetType = data.UpgradedTargetType;
    }

    private bool CheckForPlayability()
    {
        if (data.CardType == CardType.Curse || data.CardType == CardType.Quest ||
            data.CardType == CardType.Status) return false;
        
        // TODO: buff changes playability

        return currentOwner.CurrentEnergy >= currentCost;
    }
}

public enum CardState
{
    NotInit,
    Init,
    InHand,
    InDiscard,
    InDraw,
    InExhaust,
    InDeck
}
