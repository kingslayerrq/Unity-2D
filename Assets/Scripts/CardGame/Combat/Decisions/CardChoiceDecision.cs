using System;
using System.Collections.Generic;

public enum CardChoiceKind
{
    Discard,
    Exhaust
}

/// <summary>
/// The player must pick cards from a set, for example which cards to discard.
/// Answered with a <see cref="ChooseCardsCommand"/>.
///
/// Picking nothing is allowed only when <see cref="MinCount"/> is zero.
/// </summary>
public sealed class CardChoiceDecision : ICombatDecision
{
    private readonly Action<IReadOnlyList<CardInstance>> _onChosen;

    public BasePlayerUnitInstance Player { get; }
    public CardChoiceKind Kind { get; }
    public IReadOnlyList<CardInstance> Options { get; }
    public int MinCount { get; }
    public int MaxCount { get; }
    public bool IsOptional => MinCount == 0;

    /// <param name="onChosen">Runs once with the accepted picks.</param>
    public CardChoiceDecision(
        BasePlayerUnitInstance player,
        CardChoiceKind kind,
        IReadOnlyList<CardInstance> options,
        int minCount,
        int maxCount,
        Action<IReadOnlyList<CardInstance>> onChosen)
    {
        Player = player;
        Kind = kind;
        Options = options ?? Array.Empty<CardInstance>();
        MaxCount = Math.Max(0, Math.Min(maxCount, Options.Count));
        MinCount = Math.Max(0, Math.Min(minCount, MaxCount));
        _onChosen = onChosen;
    }

    /// <summary>
    /// Checks picks against the rules of this choice: the right number of
    /// cards, each one an option, none repeated.
    /// </summary>
    public bool IsValid(IReadOnlyList<CardInstance> picks, out string reason)
    {
        int count = picks?.Count ?? 0;

        if (count < MinCount)
        {
            reason = $"At least {MinCount} card(s) must be chosen; got {count}.";
            return false;
        }

        if (count > MaxCount)
        {
            reason = $"At most {MaxCount} card(s) may be chosen; got {count}.";
            return false;
        }

        for (int i = 0; i < count; i++)
        {
            CardInstance pick = picks[i];

            if (!IsOption(pick))
            {
                reason = "A chosen card is not one of the options.";
                return false;
            }

            for (int j = 0; j < i; j++)
            {
                if (ReferenceEquals(picks[j], pick))
                {
                    reason = "The same card was chosen twice.";
                    return false;
                }
            }
        }

        reason = null;
        return true;
    }

    /// <summary>
    /// The smallest valid answer: the first <see cref="MinCount"/> options.
    /// Useful as a fallback, and for bots that do not care about the choice.
    /// </summary>
    public List<CardInstance> GetMinimalPicks()
    {
        List<CardInstance> picks = new List<CardInstance>();

        for (int i = 0; i < MinCount; i++)
        {
            picks.Add(Options[i]);
        }

        return picks;
    }

    /// <summary>Applies accepted picks. Called by combat, not by players.</summary>
    internal void Resolve(IReadOnlyList<CardInstance> picks)
    {
        _onChosen?.Invoke(picks ?? Array.Empty<CardInstance>());
    }

    private bool IsOption(CardInstance card)
    {
        if (card == null) return false;

        for (int i = 0; i < Options.Count; i++)
        {
            if (ReferenceEquals(Options[i], card)) return true;
        }

        return false;
    }

    public override string ToString() => $"CardChoice({Kind}, {MinCount}-{MaxCount} of {Options.Count})";
}
