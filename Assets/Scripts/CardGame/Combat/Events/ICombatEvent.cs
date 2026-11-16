/// <summary>
/// Something that happened in combat, published on the
/// <see cref="CombatActionQueue"/> in the order it happened.
///
/// Events carry model objects only. Combat logic never waits for anyone to
/// react to them, so a combat runs the same with or without listeners.
/// </summary>
public interface ICombatEvent
{
}
