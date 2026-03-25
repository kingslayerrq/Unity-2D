using UnityEngine;

public class ResetBlockEvent
{

    public UnitInstance Source { get; }
    public UnitInstance Target { get; }
    public BlockResetSource ResetSource { get; }

    public ResetBlockEvent(UnitInstance source, UnitInstance target, BlockResetSource resetSource)
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
