using UnityEngine;

public class HealEvent
{
    public UnitInstance Source { get; }
    public UnitInstance Target { get; }
    public int Amount { get; set; }
    
    public HealEvent(UnitInstance source, UnitInstance target, int amount)
    {
        Source = source;
        Target = target;
        Amount = amount;
    }
}
