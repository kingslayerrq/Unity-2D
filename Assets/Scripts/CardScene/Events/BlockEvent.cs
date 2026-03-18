using UnityEngine;

public class BlockEvent
{
    public Unit Source { get; }
    public Unit Target { get; }
    public float Amount { get; set; }
    public bool IsFromCard { get; }

    public BlockEvent(Unit source, Unit target, float amount,  bool isFromCard)
    {
        Source = source;
        Target = target;
        Amount = amount;
        IsFromCard = isFromCard;
    }
}
