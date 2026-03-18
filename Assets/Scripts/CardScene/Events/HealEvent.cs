using UnityEngine;

public class HealEvent
{
    public Unit Source { get; }
    public Unit Target { get; }
    public int Amount { get; set; }
    
    public HealEvent(Unit source, Unit target, int amount)
    {
        Source = source;
        Target = target;
        Amount = amount;
    }
}
