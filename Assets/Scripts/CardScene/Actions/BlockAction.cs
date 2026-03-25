using System.Collections;
using UnityEngine;

public class BlockAction : ICombatAction
{
    private readonly CombatSystem _combatSystem;
    private readonly UnitInstance _source;
    private readonly UnitInstance _target;
    private readonly bool _isFromCard;
    private readonly int _amount;

    public BlockAction(CombatSystem combatSystem, UnitInstance source, UnitInstance target, int amount, bool isFromCard)
    {
        _combatSystem = combatSystem;
        _source = source;
        _target = target;
        _amount = amount;
        _isFromCard = isFromCard;
    }
    
    public IEnumerator ExecuteAction()
    {
        _combatSystem.ResolveBlocks(_source, _target, _amount,  _isFromCard);
        
        yield return null;
    }
}
