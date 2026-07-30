using System;

/// <summary>
/// Some card effects require conditions to be met to trigger corresponding effects
/// </summary>
[Serializable]
public abstract class BaseCardCondition
{
    public abstract bool IsMet(CardContext context);
}

public enum NumericComparison
{
    Equal,
    NotEqual,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual
}