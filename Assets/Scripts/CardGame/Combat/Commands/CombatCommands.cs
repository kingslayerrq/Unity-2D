using System;
using System.Collections.Generic;

/// <summary>
/// A player's answer to the pending <see cref="ICombatDecision"/>. Submitted
/// through <see cref="CombatManager.Submit"/>, which is the single way a person
/// or a bot acts on a combat.
/// </summary>
public interface ICombatCommand
{
}

/// <summary>Play a card from hand at the given targets.</summary>
public sealed class PlayCardCommand : ICombatCommand
{
    public CardInstance Card { get; }
    public IReadOnlyList<BaseUnitInstance> Targets { get; }

    public PlayCardCommand(CardInstance card, IReadOnlyList<BaseUnitInstance> targets)
    {
        Card = card;
        Targets = targets ?? Array.Empty<BaseUnitInstance>();
    }

    public override string ToString() => $"PlayCard({Card?.CurrentName}, {Targets.Count} target(s))";
}

/// <summary>End the player's turn.</summary>
public sealed class EndTurnCommand : ICombatCommand
{
    public override string ToString() => "EndTurn";
}

/// <summary>Answer a <see cref="CardChoiceDecision"/> with the chosen cards.</summary>
public sealed class ChooseCardsCommand : ICombatCommand
{
    public IReadOnlyList<CardInstance> Cards { get; }

    public ChooseCardsCommand(IReadOnlyList<CardInstance> cards)
    {
        Cards = cards ?? Array.Empty<CardInstance>();
    }

    public override string ToString() => $"ChooseCards({Cards.Count})";
}

/// <summary>
/// Whether a command was carried out. A rejected command changes nothing.
/// </summary>
public readonly struct CommandResult
{
    public bool Accepted { get; }

    /// <summary>Why the command was rejected; null when it was accepted.</summary>
    public string Reason { get; }

    private CommandResult(bool accepted, string reason)
    {
        Accepted = accepted;
        Reason = reason;
    }

    public static CommandResult Ok() => new CommandResult(true, null);
    public static CommandResult Rejected(string reason) => new CommandResult(false, reason);

    public override string ToString() => Accepted ? "Accepted" : $"Rejected: {Reason}";
}
