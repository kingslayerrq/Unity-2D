using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class BaseEnemyIntentConfig
{
   [SerializeField] protected TargetType targetType;
   public TargetType TargetType => targetType;
}
