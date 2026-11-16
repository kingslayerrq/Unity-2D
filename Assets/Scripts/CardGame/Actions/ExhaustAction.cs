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

    public void Execute()
    {
        if (_card == null) return;

        CombatActionQueue queue = _source.CombatManager.ActionQueue;

        queue.Publish(new CardsLeavingHandEvent(_source, new[] { _card }));

        _combatSystem.ResolveExhaust(_source, CardMoveCause.CardEffect, _card);

        queue.Publish(new CardExhaustedEvent(_source, _card));
    }

    public string GetActionDetails()
    {
        return $"ExhaustAction: Source-{_source} | Card-{_card?.CurrentName}";
    }
}
