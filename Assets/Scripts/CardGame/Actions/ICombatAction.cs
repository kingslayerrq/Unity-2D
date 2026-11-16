/// <summary>
/// One queued step of combat logic.
///
/// An action runs to completion inside <see cref="Execute"/>. It must not wait
/// for frames, animations or input, and must not call into views. To show
/// something on screen it publishes an <see cref="ICombatEvent"/> on the
/// <see cref="CombatActionQueue"/>; to wait for a choice it suspends the queue.
/// </summary>
public interface ICombatAction
{
    void Execute();
    string GetActionDetails();
}
