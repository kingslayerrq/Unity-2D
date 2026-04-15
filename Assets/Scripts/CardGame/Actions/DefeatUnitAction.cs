using System.Collections;
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

    public IEnumerator ExecuteAction()
    {
        _combatSystem.ResolveDefeatUnit(_defeatedBy, _defeated);
        yield return null;
    }

    public string GetActionDetails()
    {
        return $"DefeatUnitAction: {_defeatedBy.CurrentName} defeated {_defeated.CurrentName}";
    }
}
