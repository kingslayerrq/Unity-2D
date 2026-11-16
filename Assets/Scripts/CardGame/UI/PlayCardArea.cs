using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// Lets a card be dropped on open space. Only cards with no unit to choose
/// (self, a whole side, or no target) can be played this way; that rule, and
/// who the card then hits, comes from <see cref="CardTargeting"/>.
/// </summary>
// For graphics raycast
[RequireComponent(typeof(Image))]
public class PlayCardArea : MonoBehaviour, ICardDropReceiver
{
    [FormerlySerializedAs("combatManager")]
    [SerializeField] private CombatPresenter combatPresenter;

    private IReadOnlyList<BaseUnitInstance> highlightedTargets;
    public int Priority { get; private set; } = CONSTANTS.PLAY_AREA_TARGET_PRIORITY;

    public List<BaseUnitInstance> GetReceiverUnitInstance(BaseUnitInstance source, CardInstance cardInstance)
    {
        // Worked out from the rules at the moment of the drop, not from
        // whatever happened to be highlighted.
        return TryResolveDrop(source, cardInstance, out IReadOnlyList<BaseUnitInstance> targets)
            ? new List<BaseUnitInstance>(targets)
            : new List<BaseUnitInstance>();
    }

    public bool CanReceiveCard(BaseUnitInstance source, CardInstance cardInstance)
    {
        return TryResolveDrop(source, cardInstance, out _);
    }

    public List<ICardTarget> ValidateCardTargets(BaseUnitInstance source, CardInstance cardInstance)
    {
        List<ICardTarget> targets = new List<ICardTarget>();

        if (TryResolveDrop(source, cardInstance, out IReadOnlyList<BaseUnitInstance> resolved))
        {
            targets.AddRange(resolved);
        }

        return targets;
    }

    public void OnHoverEnter(BaseUnitInstance source, CardInstance cardInstance)
    {
        ClearHighlight();

        if (!TryResolveDrop(source, cardInstance, out IReadOnlyList<BaseUnitInstance> targets)) return;

        highlightedTargets = targets;
        foreach (BaseUnitInstance target in targets)
        {
            target.TriggerHighlight(true);
        }
    }

    public void OnHoverExit(BaseUnitInstance source, CardInstance cardInstance)
    {
        ClearHighlight();
    }

    private void ClearHighlight()
    {
        if (highlightedTargets == null) return;

        foreach (BaseUnitInstance target in highlightedTargets)
        {
            target.TriggerHighlight(false);
        }

        highlightedTargets = null;
    }

    private bool TryResolveDrop(
        BaseUnitInstance source,
        CardInstance cardInstance,
        out IReadOnlyList<BaseUnitInstance> targets)
    {
        targets = null;
        if (combatPresenter == null) return false;

        return CardTargeting.TryResolveDrop(
            cardInstance,
            source,
            combatPresenter.Combat.UnitsManager,
            null,
            out targets);
    }
}
