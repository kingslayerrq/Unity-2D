using System;
using UnityEngine;

[Serializable]
public abstract class BaseCardEffect
{
    public abstract Type RequiredConfigType { get; }

    public abstract void Execute(CardContext context, CardEffectData effectData);
}
