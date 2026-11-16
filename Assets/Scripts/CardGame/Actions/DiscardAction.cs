using System.Collections.Generic;
using System.Linq;

public class DiscardAction : ICombatAction
{
    private readonly CombatSystem _combatSystem;
    private readonly BasePlayerUnitInstance _source;
    private readonly List<BasePlayerUnitInstance> _targets;
    private readonly IReadOnlyList<CardInstance> _cards;
    private CardMoveCause _moveCause;

    public DiscardAction(CombatSystem combatSystem, BasePlayerUnitInstance source,
        List<BasePlayerUnitInstance> targets, IReadOnlyList<CardInstance> cards,
        CardMoveCause moveCause)
    {
        _combatSystem = combatSystem;
        _source = source;
        _targets = targets;
        _cards = cards?.ToList();
        _moveCause = moveCause;
    }

    public void Execute()
    {
        if (_cards == null || _cards.Count == 0) return;

        CombatActionQueue queue = _source.CombatManager.ActionQueue;

        queue.Publish(new CardsLeavingHandEvent(_source, _cards));

        _combatSystem.ResolveDiscard(_source, _targets, _cards, _moveCause);

        queue.Publish(new CardsDiscardedEvent(_source, _cards, _moveCause));
    }

    public string GetActionDetails()
    {
        return $"DiscardAction: Source-{_source} | Targets-{string.Join(", ", _targets)}" +
               $" | Cards-{string.Join(", ", _cards)}";
    }

}
