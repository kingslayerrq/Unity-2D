using UnityEngine;

public class HealEvent
{
    public BaseUnitInstance Source { get; }
    public BaseUnitInstance Target { get; }
    public int Amount { get; set; }
    
    public HealEvent(BaseUnitInstance source, BaseUnitInstance target, int amount)
    {
        Source = source;
        Target = target;
        Amount = amount;
    }
}
