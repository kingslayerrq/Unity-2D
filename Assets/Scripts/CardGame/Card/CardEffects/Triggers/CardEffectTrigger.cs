
public enum CardEffectTrigger
{
    OnPlay = 0,
    OnDiscard = 1,
    OnExhaust = 2
}

public enum CardMoveCause
{
    None,
    CardEffect,
    PlayCleanup,
    EndTurn,
    HandOverflow
}