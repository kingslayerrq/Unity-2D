using UnityEngine;

public class ResetBlockEvent
{

    public BaseUnitInstance Source { get; }
    public BaseUnitInstance Target { get; }
    public BlockResetSource ResetSource { get; }

    public ResetBlockEvent(BaseUnitInstance source, BaseUnitInstance target, BlockResetSource resetSource)
    {
        Source = source;
        Target = target;
        ResetSource = resetSource;
    }
}

public enum BlockResetSource
{
    None,
    TurnStart,
    CardEffect,
    BuffEffect,
    TurnEnd
}
