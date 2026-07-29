using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CardInstance
{
    [SerializeField] private CardData data;
    [SerializeField] private int currentCost;
    [SerializeField] private bool currentIsUpgraded;
    [SerializeField] private bool currentIsEnchanted;
    
    [NonSerialized] private BasePlayerUnitInstance currentOwner;

    #region Dynamic Properties
    public CardData Data => data;
    public CardState CurrentCardState { get; set; }
    public BasePlayerUnitInstance CurrentOwner => currentOwner;
    
    // Dynamic Cost (Can be permanently or temporarily modified during combat)
    public int CurrentCost => currentCost;
    
    // Computed properties automatically switch based on upgrade status
    public string CurrentName => currentIsUpgraded ? data.UpgradedCardName : data.CardName;
    public string CurrentDescription => currentIsUpgraded ? data.UpgradedCardDescription : data.CardDescription;
    public int CurrentPrice => currentIsUpgraded ? data.UpgradedCardPrice : data.CardPrice;
    public int CurrentLevel => data.CardLevel;
    public CardType CurrentCardType => data.CardType;
    public bool CurrentIsUpgraded => currentIsUpgraded;
    public bool CurrentIsEnchanted => currentIsEnchanted;
    public Sprite CurrentSprite => currentIsUpgraded ? data.UpgradedCardSprite : data.CardSprite;
    public TargetType CurrentTargetType => currentIsUpgraded ? data.UpgradedTargetType : data.TargetType;
    public IReadOnlyList<CardEffectData> CurrentCardEffects => currentIsUpgraded ? data.UpgradedCardEffects : data.CardEffects;
    public IReadOnlyList<KeywordData> CurrentKeywords => currentIsUpgraded ? data.UpgradedKeywords : data.Keywords;

    public bool CanBePlayed => CheckForPlayability();
    public bool RequireTarget => CurrentTargetType != TargetType.None;
    #endregion

    #region Constructors
    public CardInstance(CardData data, BasePlayerUnitInstance owner = null, bool isUpgraded = false, bool isEnchanted = false)
    {
        this.data = data;
        this.currentOwner = owner;
        this.CurrentCardState = CardState.Init;
        this.currentIsEnchanted = isEnchanted;
        
        if (isUpgraded)
        {
            Upgrade();
        }
        else
        {
            this.currentIsUpgraded = false;
            this.currentCost = data.CardCost;
        }
    }
    #endregion

    #region Actions & Modifiers
    public void SetOwner(BasePlayerUnitInstance owner)
    {
        this.currentOwner = owner;
    }

    public void ModifyCost(int amount)
    {
        currentCost = Mathf.Max(0, amount);
    }

    public void AddCost(int amount)
    {
        currentCost += amount;
    }
    
    public void ReduceCost(int amount)
    {
        currentCost = Mathf.Max(0, currentCost - amount);
    }

    public void Upgrade()
    {
        if (currentIsUpgraded) return;
        currentIsUpgraded = true;
        currentCost = data.UpgradedCardCost;
    }

    public CardInstance Clone()
    {
        CardInstance clone = new CardInstance(this.data, this.currentOwner, this.currentIsUpgraded, this.currentIsEnchanted)
        {
            currentCost = this.currentCost // Retains combat cost modifications
        };
        return clone;
    }

    public bool IsExhaustOnPlay()
    {
        var effects = CurrentCardEffects;
        if (effects == null) return false;

        foreach (var effectData in effects)
        {
            if (effectData?.CardEffect is not ExhaustCardEffect) continue;
            if (effectData.CardEffectConfig is not ExhaustCardEffectConfig config) return true;
            return config.ExhaustMode == ExhaustMode.ThisCard;
        }
        return false;
    }

    private bool CheckForPlayability()
    {
        if (data.CardType == CardType.Curse || 
            data.CardType == CardType.Quest || 
            data.CardType == CardType.Status) 
            return false;
        
        if (currentOwner == null) return false;

        return currentOwner.CurrentEnergy >= currentCost;
    }
    #endregion
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
