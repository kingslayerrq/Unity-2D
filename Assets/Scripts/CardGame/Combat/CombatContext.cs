/// <summary>
/// Receives what happens in combat that should count towards long-term
/// progression. Combat reports to it and never looks progression up itself.
/// </summary>
public interface ICombatProgressRecorder
{
    void RecordCardPlayed(CardInstance card);
}

/// <summary>
/// Everything a combat is given from outside. The combat core reaches for no
/// global state: its random numbers and its progression recording come from
/// here, so the same context always produces the same combat.
/// </summary>
public sealed class CombatContext
{
    /// <summary>
    /// The source of every random choice in the combat: shuffles, enemy
    /// intents and random card effects.
    /// </summary>
    public System.Random Rng { get; }

    /// <summary>Where card plays are reported. Null records nothing.</summary>
    public ICombatProgressRecorder Progress { get; }

    /// <param name="rng">Null gives an unseeded stream.</param>
    /// <param name="progress">Null records nothing.</param>
    public CombatContext(System.Random rng = null, ICombatProgressRecorder progress = null)
    {
        Rng = rng ?? new System.Random();
        Progress = progress;
    }
}
