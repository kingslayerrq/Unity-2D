using UnityEngine;

public class TempStrengthBuffInstance : StrengthBuffInstance
{
    public TempStrengthBuffInstance(BuffData data, int stacks) : base(data, stacks)
    {
    }

    public override void OnTurnEnd(BaseUnitInstance owner, CombatSystem combatSystem)
    {
        RemoveStacks(Stacks);
        Debug.Log("Temporary Strength expired and was removed.");
    }
}
