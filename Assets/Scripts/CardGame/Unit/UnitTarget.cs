using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lets a card be dropped on this unit. What the drop means, and whether it
/// is allowed, is decided by <see cref="CardTargeting"/>; this component only
/// asks and highlights.
/// </summary>
public class UnitTarget : MonoBehaviour, ICardDropReceiver
{
    private BaseUnitInstance _unitInstance;
    private IReadOnlyList<BaseUnitInstance> _highlightedTargets;

    public int Priority { get; private set; } = CONSTANTS.UNIT_TARGET_PRIORITY;

    public void Initialize(BaseUnitInstance unitInstance)
    {
        _unitInstance = unitInstance;
    }

    public List<BaseUnitInstance> GetReceiverUnitInstance(BaseUnitInstance source, CardInstance cardInstance)
    {
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

        // Highlight everything the card would hit, which for a card that
        // targets a whole side is more than this unit.
        if (!TryResolveDrop(source, cardInstance, out IReadOnlyList<BaseUnitInstance> targets)) return;

        _highlightedTargets = targets;
        foreach (BaseUnitInstance target in targets)
        {
            target.TriggerHighlight(true);
        }
    }

    public void OnHoverExit(BaseUnitInstance source, CardInstance cardInstance)
    {
        ClearHighlight();
        _unitInstance?.TriggerHighlight(false);
    }

    private void ClearHighlight()
    {
        if (_highlightedTargets == null) return;

        foreach (BaseUnitInstance target in _highlightedTargets)
        {
            target.TriggerHighlight(false);
        }

        _highlightedTargets = null;
    }

    private bool TryResolveDrop(
        BaseUnitInstance source,
        CardInstance cardInstance,
        out IReadOnlyList<BaseUnitInstance> targets)
    {
        targets = null;

        UnitsManager units = _unitInstance?.CombatManager?.UnitsManager;
        if (units == null) return false;

        return CardTargeting.TryResolveDrop(cardInstance, source, units, _unitInstance, out targets);
    }
}
