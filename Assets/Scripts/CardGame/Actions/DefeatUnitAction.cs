using UnityEngine;

public class DefeatUnitAction : ICombatAction
{
    private readonly CombatSystem _combatSystem;
    private readonly BaseUnitInstance _defeatedBy;
    private readonly BaseUnitInstance _defeated;

    public DefeatUnitAction(CombatSystem combatSystem, BaseUnitInstance defeatedBy, BaseUnitInstance defeated)
    {
        _combatSystem = combatSystem;
        _defeatedBy = defeatedBy;
        _defeated = defeated;
    }

    public void Execute()
    {
        _combatSystem.ResolveDefeatUnit(_defeatedBy, _defeated);
    }

    public string GetActionDetails()
    {
        return $"DefeatUnitAction: {_defeatedBy.CurrentName} defeated {_defeated.CurrentName}";
    }
}
