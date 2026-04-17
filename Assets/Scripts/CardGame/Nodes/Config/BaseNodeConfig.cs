using System;
using UnityEngine;

public abstract class BaseNodeConfig : ScriptableObject
{
    [SerializeField] private Sprite nodeSprite;
    [SerializeField] private string nodeName;
    
    public Sprite NodeSprite => nodeSprite;
    public string NodeName => nodeName;
    
    public abstract NodeType NodeType { get; }

    public abstract BaseNode CreateNode();
}
