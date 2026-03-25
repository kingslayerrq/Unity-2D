using UnityEngine;

public abstract class BuffData : ScriptableObject
{
    [Header("Buff Info")]
    [SerializeField] private string buffName;
    [SerializeField, TextArea] private string buffDescription;
    [SerializeField] private Sprite buffIcon;
    [SerializeField] private int maxStacks = 99;
    [SerializeField] private bool isStackable = true;
    [SerializeField] private bool isDebuff;
    [SerializeField] private bool expiresAtEndOfTurn;
    
    public string BuffName => buffName;
    public Sprite BuffIcon => buffIcon;
    public int MaxStacks => maxStacks;
    public bool IsStackable => isStackable;
    public bool IsDebuff => isDebuff;
    public bool ExpiresAtEndOfTurn => expiresAtEndOfTurn;

    public abstract BuffInstance CreateInstance(int stacks);
    
    public virtual string GetDescription(int stacks)
    {
        return buffDescription;
    }

}


