using UnityEngine;

[CreateAssetMenu(fileName = "New Buff Data", menuName = "Cards/VulnerableBuffData")]
public class VulnerableBuffData : BuffData
{
    public override BuffInstance CreateInstance(int stacks)
    {
        return new VulnerableBuffInstance(this, stacks);
    }
    public override string GetDescription(int stacks)
    {
        return $"Receives 50% more damage from attacks for {stacks} turns.";
    }
}
