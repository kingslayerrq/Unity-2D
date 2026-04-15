using System;
using UnityEngine;

[Serializable]
public abstract class BaseCardEffectConfig
{
    [Tooltip("Secondary target for the effect (e.g., self damage, or granting yourself block before attacking).")]
    [SerializeField] protected TargetType secondaryTarget = TargetType.None;
    
    public TargetType SecondaryTarget => secondaryTarget;
}
