using System.Collections;
using System.Collections.Generic;

public class PlayToExhaustAction : ICombatAction
{
    private readonly CombatSystem _combatSystem;
    private readonly BasePlayerUnitInstance _source;
    private readonly CardInstance _card;

    public PlayToExhaustAction(CombatSystem combatSystem, BasePlayerUnitInstance source, CardInstance card)
    {
        _combatSystem = combatSystem;
        _source = source;
        _card = card;
    }

    public IEnumerator ExecuteAction()
    {
        yield return _source.CombatManager.PlayerHandView.PlayToDiscardAnimation(_card);

        _combatSystem.ResolveExhaust(_source, _card);

        _source.CombatManager.PlayerHandView.CompleteAnimateOut(_card);
        _source.CombatManager.PlayerHandView.RefreshHand(_source.Hand.Cards);
    }

    public string GetActionDetails()
    {
        return $"PlayToExhaustAction: Card-{_card?.CurrentName}";
    }
}
