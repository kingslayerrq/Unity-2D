using System.Collections.Generic;
using UnityEngine;

public class BlockAction : ICombatAction
{
    private readonly CombatSystem _combatSystem;
    private readonly BaseUnitInstance _source;
    private readonly IReadOnlyList<BaseUnitInstance> _targets;
    private readonly bool _isFromCard;
    private readonly int _amount;

    public BlockAction(CombatSystem combatSystem, BaseUnitInstance source, IReadOnlyList<BaseUnitInstance> targets, int amount, bool isFromCard)
    {
        _combatSystem = combatSystem;
        _source = source;
        _targets = targets;
        _amount = amount;
        _isFromCard = isFromCard;
    }
    
    public void Execute()
    {
        _combatSystem.ResolveBlocks(_source, _targets, _amount,  _isFromCard);
    }

    public string GetActionDetails()
    {
        return $"BlockAction: Source-{_source} | Targets-{string.Join(", ", _targets)} | " +
               $"Amount-{_amount}  | IsFromCard-{_isFromCard}";
    }
}
