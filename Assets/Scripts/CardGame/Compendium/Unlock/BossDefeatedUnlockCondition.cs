using UnityEngine;

[CreateAssetMenu(
    menuName = "Progression/Unlock Conditions/Boss Defeated")]
public sealed class BossDefeatedUnlockCondition : UnlockCondition
{
    [SerializeField]
    private EnemyUnitData bossData;

    [SerializeField]
    private string bossDisplayName;

    public override string Description =>
        $"Defeat {bossDisplayName}.";

    public override bool IsMet(
        GlobalProgressionManager progression)
    {
        return progression.HasDefeatedBossGlobal(bossData.EnemyId);
    }
}