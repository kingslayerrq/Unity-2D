using UnityEngine;

[CreateAssetMenu(fileName = "Strength Buff Data", menuName = "BuffData/Strength Buff Data")]
public class StrengthBuffData : BuffData
{
    public override BuffInstance CreateInstance(int stacks)
    {
        return new StrengthBuffInstance(this, stacks);
    }
}


