using UnityEngine;

public class DamageEvent 
{
    public Unit Source { get; }
    public Unit Target { get; }
    public int Amount { get; set; }
    public CardType CardType { get; }

    public DamageEvent(Unit source, Unit target, int amount, CardType cardType)
    {
        Source = source;
        Target = target;
        Amount = amount;
        CardType = cardType;
    }
    
}
