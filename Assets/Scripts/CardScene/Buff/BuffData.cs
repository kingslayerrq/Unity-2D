using UnityEngine;

public abstract class BuffData : ScriptableObject
{
    [Header("Buff Info")]
    [SerializeField] private string buffName;
    [SerializeField] private string buffDescription;
    [SerializeField] private Sprite buffIcon;

    public abstract BuffInstance CreateInstance(int stacks);

}


