using System.Collections.Generic;

public class PlayCardAction : ICombatAction
{
    private readonly CombatSystem _combatSystem;
    private readonly BasePlayerUnitInstance _source;
    private readonly List<BaseUnitInstance> _targets;
    private readonly CardInstance _card;

    public PlayCardAction(CombatSystem combatSystem, BasePlayerUnitInstance source, List<BaseUnitInstance> targets, CardInstance card)
    {
        _combatSystem = combatSystem;
        _source = source;
        _targets = targets;
        _card = card;
    }

    public void Execute()
    {
        CombatActionQueue queue = _source.CombatManager.ActionQueue;

        queue.Publish(new CardsLeavingHandEvent(_source, new[] { _card }));

        _combatSystem.ResolvePlayCard(_source, _targets, _card);

        // Queued behind the effects the card just queued, so listeners see the
        // card finish only after those effects have resolved.
        queue.Enqueue(new PublishEventAction(queue, new CardPlayResolvedEvent(_source, _card)), null);
    }

    public string GetActionDetails()
    {
        return $"PlayCardAction: Source-{_source} | Targets-{(_targets == null ? "none" : string.Join(", ", _targets))}" +
               $" | Card-{_card?.CurrentName}";
    }
}
