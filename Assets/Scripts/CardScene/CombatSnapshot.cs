using System.Collections.Generic;
using UnityEngine;

public class CombatSnapshot
{
    public BaseEnemyUnitInstance Self { get; private set; }
    public IReadOnlyList<BaseEnemyUnitInstance> AllEnemies { get; private set; }
    public IReadOnlyList<BasePlayerUnitInstance> AllPlayers { get; private set; }
    public int CurrentTurn { get; private set; }
    
    public CombatSnapshot(BaseEnemyUnitInstance self, IReadOnlyList<BaseEnemyUnitInstance> enemies, IReadOnlyList<BasePlayerUnitInstance> players, int currentTurn)
    {
        Self = self;
        AllPlayers = players;
        AllEnemies = enemies;
        CurrentTurn = currentTurn;
    }
}
