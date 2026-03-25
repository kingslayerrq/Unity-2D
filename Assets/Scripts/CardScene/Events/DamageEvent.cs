using UnityEngine;

public class DamageEvent 
{
    public UnitInstance Source { get; }
    public UnitInstance Target { get; }
    public int Amount { get; set; }
    public CardType CardType { get; }

    public DamageEvent(UnitInstance source, UnitInstance target, int amount, CardType cardType)
    {
        Source = source;
        Target = target;
        Amount = amount;
        CardType = cardType;
    }
    
}
