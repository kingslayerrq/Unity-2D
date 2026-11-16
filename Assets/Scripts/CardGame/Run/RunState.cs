/// <summary>
/// Where a run currently is. <see cref="WinState"/> covers both a combat
/// that was just won (reward on offer) and a run that was won outright.
/// </summary>
public enum RunState
{
    None,
    SelectPlayerUnitState,
    SetupActState,
    MapState,
    CombatState,
    WinState,
    LoseState
}
