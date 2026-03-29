using System;
using UnityEngine;

[Serializable]
public class CardInstance
{
    
    public bool CanBePlayed => CheckForPlayability();
    [SerializeField] private CardData data;
    [SerializeField] private UnitInstance currentOwner;
    [SerializeField] private int currentCost;
    [SerializeField] private string currentName;
    [SerializeField] private string currentDescription;
    [SerializeField] private int currentPrice;
    [SerializeField] private int currentLevel;
    [SerializeField] private Sprite currentSprite;
    [SerializeField] private TargetType currentTargetType;
    

    #region getters
    public CardData Data => data;
    public UnitInstance CurrentOwner => currentOwner;
    public int CurrentCost => currentCost;
    public string CurrentName => currentName;
    public string CurrentDescription => currentDescription;
    public int CurrentPrice => currentPrice;
    public int CurrentLevel => currentLevel;
    public Sprite CurrentSprite => currentSprite;
    public TargetType CurrentTargetType => currentTargetType;
    

    #endregion
    

    public CardInstance(CardData data, UnitInstance owner)
    {
        // ref to base card data
        this.data = data;
        // instance stats
        this.currentOwner = owner;
        this.currentCost = data.CardCost;
        this.currentName = data.CardName;
        this.currentDescription = data.CardDescription;
        this.currentPrice = data.CardPrice;
        this.currentLevel = data.CardLevel;
        this.currentSprite = data.CardSprite;
        this.currentTargetType = data.TargetType;
    }
    
    public bool RequireTarget => Data.TargetType != TargetType.None;
    
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

    private bool CheckForPlayability()
    {
        if (data.CardType == CardType.Curse || data.CardType == CardType.Quest ||
            data.CardType == CardType.Status) return false;
        
        // TODO: buff changes playability

        return currentOwner.CurrentEnergy >= currentCost;
    }
}
