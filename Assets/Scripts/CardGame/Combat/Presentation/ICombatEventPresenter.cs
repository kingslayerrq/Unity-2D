using System.Collections;

/// <summary>
/// A view-side listener that shows combat events on screen.
/// </summary>
public interface ICombatEventPresenter
{
    /// <summary>
    /// Reacts to an event. Anything instant should happen inside this call.
    /// </summary>
    /// <returns>
    /// A routine (typically an animation) that combat should wait for before
    /// its next action, or null when there is nothing to wait for.
    /// </returns>
    IEnumerator Present(ICombatEvent combatEvent);
}
