using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageAction : ICombatAction
{
    private readonly CombatSystem _combatSystem;
    private readonly BaseUnitInstance _source;
    private readonly List<BaseUnitInstance> _targets;
    private readonly CardType _cardType;
    private readonly int _amount;

    public DamageAction(CombatSystem combatSystem, BaseUnitInstance source, List<BaseUnitInstance> targets, int amount, CardType cardType)
    {
        _combatSystem = combatSystem;
        _source = source;
        _targets = targets;
        _amount = amount;
        _cardType = cardType;
    }
    
    public IEnumerator ExecuteAction()
    {
        _combatSystem.ResolveDamage(_source, _targets, _amount, _cardType);
        
        yield return null;
    }

    public string GetActionDetails()
    {
        return $"DamageAction: Source-{_source} |  Targets-{string.Join(", ", _targets)} | " +
               $"Amount-{_amount} | CardType-{_cardType}";
    }

}
