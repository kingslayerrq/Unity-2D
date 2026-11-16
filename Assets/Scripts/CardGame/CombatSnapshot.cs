using System.Collections.Generic;
using UnityEngine;

public class CombatSnapshot
{
    public BaseEnemyUnitInstance Self { get; private set; }
    public IReadOnlyList<BaseEnemyUnitInstance> AllEnemies { get; private set; }
    public IReadOnlyList<BasePlayerUnitInstance> AllPlayers { get; private set; }
    public int CurrentTurn { get; private set; }

    /// <summary>The combat's random stream, for any random choice an enemy makes.</summary>
    public System.Random Rng { get; private set; }
    
    public CombatSnapshot(BaseEnemyUnitInstance self, IReadOnlyList<BaseEnemyUnitInstance> enemies, IReadOnlyList<BasePlayerUnitInstance> players, int currentTurn, System.Random rng)
    {
        Self = self;
        AllPlayers = players;
        AllEnemies = enemies;
        CurrentTurn = currentTurn;
        Rng = rng ?? new System.Random();
    }
}
