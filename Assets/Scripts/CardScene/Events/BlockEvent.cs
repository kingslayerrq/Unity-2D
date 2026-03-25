using UnityEngine;

public class BlockEvent
{
    public UnitInstance Source { get; }
    public UnitInstance Target { get; }
    public int Amount { get; set; }
    public bool IsFromCard { get; }

    public BlockEvent(UnitInstance source, UnitInstance target, int amount,  bool isFromCard)
    {
        Source = source;
        Target = target;
        Amount = amount;
        IsFromCard = isFromCard;
    }
}
