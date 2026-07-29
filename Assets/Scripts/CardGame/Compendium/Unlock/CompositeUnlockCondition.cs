using System.Collections.Generic;
using UnityEngine;

public enum UnlockConditionMode
{
    All,
    Any
}

[CreateAssetMenu(
    menuName = "Progression/Unlock Conditions/Composite")]
public sealed class CompositeUnlockCondition : UnlockCondition
{
    [SerializeField]
    private UnlockConditionMode mode = UnlockConditionMode.All;

    [SerializeField]
    private List<UnlockCondition> conditions = new();

    public override bool IsMet(
        GlobalProgressionManager progression)
    {
        if (conditions == null || conditions.Count == 0)
            return false;

        if (mode == UnlockConditionMode.All)
        {
            foreach (UnlockCondition condition in conditions)
            {
                if (condition == null ||
                    !condition.IsMet(progression))
                {
                    return false;
                }
            }

            return true;
        }

        foreach (UnlockCondition condition in conditions)
        {
            if (condition != null &&
                condition.IsMet(progression))
            {
                return true;
            }
        }

        return false;
    }
}