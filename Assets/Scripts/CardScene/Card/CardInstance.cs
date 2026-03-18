using UnityEngine;

public class CardInstance
{
    public CardData Data { get; private set; }
    public int CurrentCost { get; private set; }
    public string CurrentName { get; private set; }
    public string CurrentDescription { get; private set; }
    public int CurrentPrice { get; private set; }
    public int CurrentLevel { get; private set; }
    public Sprite CurrentSprite { get; private set; }
    public TargetType CurrentTargetType { get; private set; }
    
    public CardInstance(CardData data)
    {
        // ref to base card data
        Data = data;
        // instance stats
        CurrentCost = data.CardCost;
        CurrentName = data.CardName;
        CurrentDescription = data.CardDescription;
        CurrentPrice = data.CardPrice;
        CurrentLevel = data.CardLevel;
        CurrentSprite = data.CardSprite;
        CurrentTargetType = data.TargetType;
    }
    
    public bool RequireTarget => Data.TargetType != TargetType.None;
    
    public void ModifyCost(int amount)
    {
        CurrentCost = amount;
    }

    public void AddCost(int amount)
    {
        CurrentCost += amount;
    }
    
    public void ReduceCost(int amount)
    {
        CurrentCost -= amount;
        if  (CurrentCost < 0)
        {
            CurrentCost = 0;
        }
    }
}
