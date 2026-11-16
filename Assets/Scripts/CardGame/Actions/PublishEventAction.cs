/// <summary>
/// Publishes an event when its turn in the queue comes.
///
/// Use it for an event that must be seen only after other queued actions have
/// resolved. Example: a card's "finished playing" event belongs after the
/// effects the card queued.
/// </summary>
public class PublishEventAction : ICombatAction
{
    private readonly CombatActionQueue _queue;
    private readonly ICombatEvent _combatEvent;

    public PublishEventAction(CombatActionQueue queue, ICombatEvent combatEvent)
    {
        _queue = queue;
        _combatEvent = combatEvent;
    }

    public void Execute()
    {
        _queue?.Publish(_combatEvent);
    }

    public string GetActionDetails()
    {
        return $"PublishEventAction: {_combatEvent}";
    }
}
