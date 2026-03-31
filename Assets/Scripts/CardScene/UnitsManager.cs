using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitsManager : MonoBehaviour
{
    [SerializeField] private CombatManager combatManager;
    [SerializeField] private PlayerUnitData playerUnitData;
    [SerializeField] private List<EnemyUnitData> _enemyUnitsData;

    private void Start()
    {
        if (combatManager != null)
        {
            combatManager.OnStateChange += HandleStateChange;
        }
    }

    public void CreatePlayerUnitInstance()
    {
        BasePlayerUnitInstance playerUnitInstance = new BasePlayerUnitInstance(playerUnitData);
    }

    private void HandleStateChange(CombatManager.CombatState combatState)
    {
        switch (combatState)
        {
            case CombatManager.CombatState.BattleSetup:
                break;
            default:
                break;
        }
    }

    private void OnDestroy()
    {
        if (combatManager != null)
        {
            combatManager.OnStateChange -= HandleStateChange;
        }
    }
}
