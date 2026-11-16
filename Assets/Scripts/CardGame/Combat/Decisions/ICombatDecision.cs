/// <summary>
/// Something combat is waiting for a player to decide.
///
/// While a decision is pending, combat does nothing on its own. Whoever is
/// playing (the UI for a person, an agent for a bot) reads the decision from
/// <see cref="CombatManager.PendingDecision"/> and answers it by submitting an
/// <see cref="ICombatCommand"/>.
/// </summary>
public interface ICombatDecision
{
}
