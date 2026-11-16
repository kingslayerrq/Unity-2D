using NUnit.Framework;

/// <summary>
/// Runs real combat rules through the queue with no scene, no views and no
/// frames. Every action resolves inside the call that queued it.
/// </summary>
public sealed class HeadlessCombatTests
{
    private CombatTestRig rig;

    [SetUp]
    public void SetUp()
    {
        rig = new CombatTestRig();
    }

    [TearDown]
    public void TearDown()
    {
        rig.Dispose();
    }

    [Test]
    public void DamageLandsAsSoonAsItIsQueued()
    {
        BaseEnemyUnitInstance attacker = rig.AddEnemy("Attacker", 30);
        BaseEnemyUnitInstance target = rig.AddEnemy("Target", 20);

        rig.Queue.Enqueue(
            new DamageAction(
                rig.CombatSystem,
                attacker,
                new[] { target },
                6,
                CardType.Attack),
            null);

        Assert.That(target.CurrentHealth, Is.EqualTo(14));
        Assert.That(rig.Queue.IsIdle, Is.True);
    }

    [Test]
    public void MultiHitKillDefeatsTheTargetOnce()
    {
        BaseEnemyUnitInstance attacker = rig.AddEnemy("Attacker", 30);
        BaseEnemyUnitInstance target = rig.AddEnemy("Target", 5);

        int disposedCount = 0;
        target.OnDisposed += () => disposedCount++;

        // Queue every hit before any of them resolves, as a multi-hit card does.
        rig.Queue.AutoDrain = false;
        for (int hit = 0; hit < 3; hit++)
        {
            rig.Queue.Enqueue(
                new DamageAction(
                    rig.CombatSystem,
                    attacker,
                    new[] { target },
                    5,
                    CardType.Attack),
                null);
        }

        rig.Queue.Drain();

        Assert.That(target.IsDead, Is.True);
        Assert.That(disposedCount, Is.EqualTo(1));
        Assert.That(rig.Queue.IsIdle, Is.True);
    }

    [Test]
    public void StrengthAndVulnerableChangeAttackDamage()
    {
        BaseEnemyUnitInstance attacker = rig.AddEnemy("Attacker", 30);
        BaseEnemyUnitInstance target = rig.AddEnemy("Target", 30);

        attacker.ReceiveBuff(rig.CreateAsset<StrengthBuffData>(), 2);
        target.ReceiveBuff(rig.CreateAsset<VulnerableBuffData>(), 1);

        rig.Queue.Enqueue(
            new DamageAction(
                rig.CombatSystem,
                attacker,
                new[] { target },
                6,
                CardType.Attack),
            null);

        // (6 + 2 Strength) * 1.5 Vulnerable = 12.
        Assert.That(target.CurrentHealth, Is.EqualTo(18));
    }
}
