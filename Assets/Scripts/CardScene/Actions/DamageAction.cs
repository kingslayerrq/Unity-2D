using System.Collections;
using UnityEngine;

public class DamageAction : ICombatAction
{
    private readonly CombatSystem _combatSystem;
    private readonly UnitInstance _source;
    private readonly UnitInstance _target;
    private readonly CardType _cardType;
    private readonly int _amount;

    public DamageAction(CombatSystem combatSystem, UnitInstance source, UnitInstance target, int amount, CardType cardType)
    {
        _combatSystem = combatSystem;
        _source = source;
        _target = target;
        _amount = amount;
        _cardType = cardType;
    }
    
    public IEnumerator ExecuteAction()
    {
        _combatSystem.ResolveDamage(_source, _target, _amount, _cardType);
        
        yield return null;
    }
}
