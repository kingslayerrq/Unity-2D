using System;
using UnityEngine;

public abstract class BaseNodeConfig : ScriptableObject
{
    public abstract NodeType NodeType { get; }

    public abstract BaseNode CreateNode();
}
