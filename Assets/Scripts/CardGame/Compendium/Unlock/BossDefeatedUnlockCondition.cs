using UnityEngine;

[CreateAssetMenu(
    menuName = "Progression/Unlock Conditions/Boss Defeated")]
public sealed class BossDefeatedUnlockCondition : UnlockCondition
{
    [Tooltip("Use a stable ID, not the boss's display name.")] [SerializeField]
    private EnemyUnitData bossData;

    [SerializeField]
    private string bossDisplayName;

    public override string Description =>
        $"Defeat {bossDisplayName}.";

    public override bool IsMet(
        GlobalProgressionManager progression)
    {
        return progression.HasDefeatedBossGlobal(bossData);
    }
}