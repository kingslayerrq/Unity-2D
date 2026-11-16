using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The units taking part in a combat, and the checks for one side being wiped
/// out. Plain C#; <see cref="UnitSpawner"/> puts their views on screen.
/// </summary>
public class UnitsManager
{
    public event Action OnAllEnemiesDefeated;
    public event Action OnAllPlayersDefeated;

    public BasePlayerUnitInstance ActivePlayerUnitInstance { get; private set; } = null;
    public List<BasePlayerUnitInstance> ActivePlayerUnitInstances { get; private set; } =
        new List<BasePlayerUnitInstance>();

    public List<BaseEnemyUnitInstance> ActiveEnemyUnitInstances { get; private set; } =
        new List<BaseEnemyUnitInstance>();
    private List<BaseUnitInstance> _allActiveUnits = new List<BaseUnitInstance>();
    public IReadOnlyList<BaseUnitInstance> AllActiveUnits => _allActiveUnits;

    /// <summary>
    /// Forgets every unit, ready for a new battle. Raises no defeat events.
    /// </summary>
    public void Clear()
    {
        _allActiveUnits.Clear();
        ActivePlayerUnitInstances.Clear();
        ActiveEnemyUnitInstances.Clear();
        ActivePlayerUnitInstance = null;
    }

    /// <summary>
    /// Adds the player unit, which lives across combats, and makes it the
    /// active one.
    /// </summary>
    /// <param name="existing"></param>
    public void RegisterExistingPlayerUnit(BasePlayerUnitInstance existing)
    {
        ActivePlayerUnitInstance = existing;
        AddUnit(existing);
    }

    public void AddUnit(BaseUnitInstance unit)
    {
        _allActiveUnits.Add(unit);

        if (unit is BaseEnemyUnitInstance enemy)
        {
            ActiveEnemyUnitInstances.Add(enemy);
        }
        else if (unit is BasePlayerUnitInstance player)
        {
            ActivePlayerUnitInstances.Add(player);
        }
    }
    public void RemoveUnit(BaseUnitInstance unit)
    {
        _allActiveUnits.Remove(unit);

        if (unit is BaseEnemyUnitInstance enemy)
        {
            ActiveEnemyUnitInstances.Remove(enemy);

            // Check win condition
            if (ActiveEnemyUnitInstances.Count == 0)
            {
                OnAllEnemiesDefeated?.Invoke();
            }
        }
        else if (unit is BasePlayerUnitInstance player)
        {
            ActivePlayerUnitInstances.Remove(player);
            if (ReferenceEquals(unit, ActivePlayerUnitInstance))
            {
                ActivePlayerUnitInstance = null;
            }
            // Check lose condition
            if (ActivePlayerUnitInstances.Count == 0)
            {
                OnAllPlayersDefeated?.Invoke();
            }
        }
    }

    public void SetActivePlayerUnitInstance(BasePlayerUnitInstance unitInstance = null)
    {
        ActivePlayerUnitInstance = unitInstance;
    }

    public IReadOnlyList<BaseUnitInstance> GetUnitsByType(TargetType targetType)
    {
        IReadOnlyList<BaseUnitInstance> units = null;
        switch (targetType)
        {
            case TargetType.Self:
                units = new BaseUnitInstance[] { this.ActivePlayerUnitInstance };
                break;
            case TargetType.AllPlayer:
                units = this.ActivePlayerUnitInstances;
                break;
            case TargetType.AllEnemy:
                units = this.ActiveEnemyUnitInstances;
                break;
            default:
                Debug.LogError("Secondary target type not supported");
                break;
        }

        return units;
    }
}
