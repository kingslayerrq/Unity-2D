using UnityEngine;

public abstract class UnlockCondition : ScriptableObject
{
    [SerializeField, TextArea]
    private string description;

    public virtual string Description => description;

    public abstract bool IsMet(
        GlobalProgressionManager progression);
}