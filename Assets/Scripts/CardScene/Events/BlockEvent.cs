using UnityEngine;

public class BlockEvent
{
    public BaseUnitInstance Source { get; }
    public BaseUnitInstance Target { get; }
    public int Amount { get; set; }
    public bool IsFromCard { get; }

    public BlockEvent(BaseUnitInstance source, BaseUnitInstance target, int amount,  bool isFromCard)
    {
        Source = source;
        Target = target;
        Amount = amount;
        IsFromCard = isFromCard;
    }
}
