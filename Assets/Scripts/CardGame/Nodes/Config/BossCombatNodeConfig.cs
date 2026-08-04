using UnityEngine;

[CreateAssetMenu(fileName = "new BossCombatNodeConfig", menuName = "Nodes/Combat/BossCombatNodeConfig")]
public class BossCombatNodeConfig : BaseCombatNodeConfig
{
    [SerializeField] private string id;
    
    public override NodeType NodeType => NodeType.CombatBoss;
    public override BaseNode CreateNode() => new BossCombatNode(this);

    // Convenience property for easy access to the single boss unit
    public EnemyUnitData BossData => enemyDatas.Count > 0 ? enemyDatas[0] : null;

    private void OnValidate()
    {
        // Enforce maximum size of 1
        if (enemyDatas != null && enemyDatas.Count > 1)
        {
            Debug.LogWarning($"[{name}] BossCombatNodeConfig can only contain 1 EnemyUnitData. Extra elements removed.", this);
            enemyDatas.RemoveRange(1, enemyDatas.Count - 1);
        }
    }
}