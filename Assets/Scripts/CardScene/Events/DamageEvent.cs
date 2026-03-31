using UnityEngine;

public class DamageEvent 
{
    public BaseUnitInstance Source { get; }
    public BaseUnitInstance Target { get; }
    public int Amount { get; set; }
    public CardType CardType { get; }

    public DamageEvent(BaseUnitInstance source, BaseUnitInstance target, int amount, CardType cardType)
    {
        Source = source;
        Target = target;
        Amount = amount;
        CardType = cardType;
    }
    
}
