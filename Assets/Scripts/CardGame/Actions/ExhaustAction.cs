using System.Collections;

public class ExhaustAction : ICombatAction
{
    private readonly CombatSystem _combatSystem;
    private readonly BasePlayerUnitInstance _source;
    private readonly CardInstance _card;

    public ExhaustAction(CombatSystem combatSystem, BasePlayerUnitInstance source, CardInstance card)
    {
        _combatSystem = combatSystem;
        _source = source;
        _card = card;
    }

    public IEnumerator ExecuteAction()
    {
        if (_card == null) yield break;

        _source.CombatManager.PlayerHandView.TryBeginAnimateOut(_card, out _);

        _combatSystem.ResolveExhaust(_source, _card);

        yield return _source.CombatManager.PlayerHandView.ExhaustCardsAnimation(new[] { _card });

        _source.CombatManager.PlayerHandView.CompleteAnimateOut(_card);
        _source.CombatManager.PlayerHandView.RefreshHand(_source.Hand.Cards);
    }

    public string GetActionDetails()
    {
        return $"ExhaustAction: Source-{_source} | Card-{_card?.CurrentName}";
    }
}
