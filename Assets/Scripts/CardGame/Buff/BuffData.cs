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
    [SerializeField] private KeywordData keywordData;
    
    public string BuffName => buffName;
    public Sprite BuffIcon => buffIcon;
    public int MaxStacks => maxStacks;
    public bool IsStackable => isStackable;
    public bool IsDebuff => isDebuff;
    public bool ExpiresAtEndOfTurn => expiresAtEndOfTurn;
    public KeywordData KeywordData => keywordData;

    public abstract BuffInstance CreateInstance(int stacks);
    
    public virtual string GetDescription(int stacks)
    {
        return buffDescription;
    }
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        // Automatically sync the cardName string with the filename of the ScriptableObject
        if (!string.IsNullOrWhiteSpace(buffName) && buffName == this.name) return;
        buffName = this.name.Replace("_Buff", "").Replace("Temp_", "").Replace("_", " ");
    }
#endif

}


