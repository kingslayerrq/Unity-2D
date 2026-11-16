using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Puts the views for a combat's units on screen. The units themselves, and
/// who is alive, are kept by <see cref="UnitsManager"/>.
/// </summary>
public class UnitSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private UnitView playerPrefab;
    [SerializeField] private UnitView enemyPrefab;

    [Header("Spawn Points")]
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private Transform[] enemySpawnPoints;

    /// <summary>
    /// Replaces whatever unit views are on screen with views for the units
    /// now in the combat.
    /// </summary>
    public void SpawnUnitViews(CombatManager combat)
    {
        SpawnPlayerView(combat.Player);
        SpawnEnemyViews(combat.Enemies);
    }

    private void SpawnPlayerView(BasePlayerUnitInstance player)
    {
        if (player == null)
        {
            Debug.LogError("No active player unit");
            return;
        }
        foreach (Transform child in playerSpawnPoint)
            Destroy(child.gameObject);

        UnitView playerUnitView = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity, playerSpawnPoint);

        // Link the view to the model
        playerUnitView.Initialize(player);
    }

    private void SpawnEnemyViews(IReadOnlyList<BaseEnemyUnitInstance> enemies)
    {
        foreach (Transform spawnPoint in enemySpawnPoints)
            foreach (Transform child in spawnPoint)
                Destroy(child.gameObject);

        for (int i = 0; i < enemies.Count; i++)
        {
            // TODO: Decide spawnpoint logic
            Transform spawnPoint = (i < enemySpawnPoints.Length) ? enemySpawnPoints[i] : transform;

            UnitView enemyView = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity, spawnPoint);

            enemyView.Initialize(enemies[i]);
        }
    }
}
