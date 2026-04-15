using UnityEngine;

[CreateAssetMenu(fileName = "Temp Strength Buff Data", menuName = "BuffData/Temp Strength Buff Data")]
public class TempStrengthBuffData : StrengthBuffData
{
    public override BuffInstance CreateInstance(int stacks)
    {
        return new TempStrengthBuffInstance(this, stacks);
    }
}
