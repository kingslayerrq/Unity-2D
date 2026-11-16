using System;
using System.Collections.Generic;

/// <summary>
/// The rules for what a card may be played at. This is the only place those
/// rules live: decisions list their options from it, commands are checked
/// against it, and the drop receivers on screen ask it what a drop means.
///
/// A "target option" is one legal way to play the card: the full list of
/// units it would be played at. A card that hits one enemy has one option per
/// living enemy. A card that hits all enemies has a single option holding all
/// of them.
/// </summary>
public static class CardTargeting
{
    /// <summary>
    /// Every legal way to target the card right now. Empty when the card has
    /// nothing it can be played at.
    /// </summary>
    public static List<IReadOnlyList<BaseUnitInstance>> GetTargetOptions(
        CardInstance card,
        BaseUnitInstance source,
        UnitsManager units)
    {
        List<IReadOnlyList<BaseUnitInstance>> options = new List<IReadOnlyList<BaseUnitInstance>>();
        if (card == null || units == null) return options;

        switch (card.CurrentTargetType)
        {
            case TargetType.None:
                // Played at nothing: one option with no targets.
                options.Add(Array.Empty<BaseUnitInstance>());
                break;

            case TargetType.Self:
                if (IsAlive(source)) options.Add(new[] { source });
                break;

            case TargetType.Enemy:
                AddOnePerUnit(options, units.ActiveEnemyUnitInstances);
                break;

            case TargetType.Player:
                AddOnePerUnit(options, units.ActivePlayerUnitInstances);
                break;

            case TargetType.Any:
                AddOnePerUnit(options, units.AllActiveUnits);
                break;

            case TargetType.AllEnemy:
                AddWholeGroup(options, units.ActiveEnemyUnitInstances);
                break;

            case TargetType.AllPlayer:
                AddWholeGroup(options, units.ActivePlayerUnitInstances);
                break;
        }

        return options;
    }

    /// <summary>
    /// True when the targets are exactly one of the card's legal options,
    /// in any order.
    /// </summary>
    public static bool IsLegal(
        CardInstance card,
        BaseUnitInstance source,
        UnitsManager units,
        IReadOnlyList<BaseUnitInstance> targets)
    {
        List<IReadOnlyList<BaseUnitInstance>> options = GetTargetOptions(card, source, units);

        for (int i = 0; i < options.Count; i++)
        {
            if (SameUnits(options[i], targets)) return true;
        }

        return false;
    }

    /// <summary>
    /// What playing the card means when it is dropped on a unit, or on open
    /// space when <paramref name="droppedOn"/> is null.
    ///
    /// A card that picks one unit must be dropped on that unit. A card with
    /// no choice to make (self, a whole side, or no target) may be dropped on
    /// open space, or on any unit it would affect.
    /// </summary>
    /// <returns>False when the drop is not a legal way to play the card.</returns>
    public static bool TryResolveDrop(
        CardInstance card,
        BaseUnitInstance source,
        UnitsManager units,
        BaseUnitInstance droppedOn,
        out IReadOnlyList<BaseUnitInstance> targets)
    {
        targets = null;
        if (card == null) return false;

        List<IReadOnlyList<BaseUnitInstance>> options = GetTargetOptions(card, source, units);

        if (droppedOn == null)
        {
            if (PicksOneUnit(card.CurrentTargetType) || options.Count == 0) return false;

            targets = options[0];
            return true;
        }

        for (int i = 0; i < options.Count; i++)
        {
            if (!Contains(options[i], droppedOn)) continue;

            targets = options[i];
            return true;
        }

        return false;
    }

    /// <summary>True for cards where the player chooses which single unit to hit.</summary>
    public static bool PicksOneUnit(TargetType targetType)
    {
        return targetType == TargetType.Enemy ||
               targetType == TargetType.Player ||
               targetType == TargetType.Any;
    }

    private static void AddOnePerUnit<T>(
        List<IReadOnlyList<BaseUnitInstance>> options,
        IReadOnlyList<T> units)
        where T : BaseUnitInstance
    {
        for (int i = 0; i < units.Count; i++)
        {
            if (IsAlive(units[i])) options.Add(new BaseUnitInstance[] { units[i] });
        }
    }

    private static void AddWholeGroup<T>(
        List<IReadOnlyList<BaseUnitInstance>> options,
        IReadOnlyList<T> units)
        where T : BaseUnitInstance
    {
        List<BaseUnitInstance> group = new List<BaseUnitInstance>();

        for (int i = 0; i < units.Count; i++)
        {
            if (IsAlive(units[i])) group.Add(units[i]);
        }

        if (group.Count > 0) options.Add(group);
    }

    private static bool IsAlive(BaseUnitInstance unit)
    {
        return unit != null && !unit.IsDead;
    }

    private static bool Contains(IReadOnlyList<BaseUnitInstance> units, BaseUnitInstance unit)
    {
        for (int i = 0; i < units.Count; i++)
        {
            if (ReferenceEquals(units[i], unit)) return true;
        }

        return false;
    }

    private static bool SameUnits(IReadOnlyList<BaseUnitInstance> expected, IReadOnlyList<BaseUnitInstance> actual)
    {
        int actualCount = actual?.Count ?? 0;
        if (expected.Count != actualCount) return false;

        for (int i = 0; i < actualCount; i++)
        {
            // Same size and every unit present once: reject a repeated unit.
            if (!Contains(expected, actual[i])) return false;

            for (int j = 0; j < i; j++)
            {
                if (ReferenceEquals(actual[j], actual[i])) return false;
            }
        }

        return true;
    }
}
