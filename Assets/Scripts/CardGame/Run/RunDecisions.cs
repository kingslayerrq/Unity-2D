using System.Collections.Generic;

/// <summary>
/// Something a run is waiting for the player to decide, outside combat.
/// Read from <see cref="RunSession.PendingDecision"/> and answered with an
/// <see cref="IRunCommand"/>. During a combat the run has nothing pending;
/// the combat's own decisions are on <see cref="RunSession.Combat"/>.
/// </summary>
public interface IRunDecision
{
}

/// <summary>
/// Pick the next node on the map. Answered with a <see cref="ChooseNodeCommand"/>.
/// </summary>
public sealed class ChooseNodeDecision : IRunDecision
{
    public int Floor { get; }
    public IReadOnlyList<BaseNode> Options { get; }

    public ChooseNodeDecision(int floor, IReadOnlyList<BaseNode> options)
    {
        Floor = floor;
        Options = options;
    }

    public override string ToString() => $"ChooseNode(floor {Floor}, {Options.Count} option(s))";
}

/// <summary>
/// A combat was won and cards are on offer. Answered with a
/// <see cref="TakeRewardCommand"/> or a <see cref="SkipRewardCommand"/>;
/// either one moves the run on to the map.
/// </summary>
public sealed class CardRewardDecision : IRunDecision
{
    /// <summary>The cards on offer. Empty when there is nothing left to take.</summary>
    public IReadOnlyList<CardData> Offers { get; }

    public CardRewardDecision(IReadOnlyList<CardData> offers)
    {
        Offers = offers;
    }

    public override string ToString() => $"CardReward({Offers.Count} offer(s))";
}

/// <summary>A player's answer to the run's pending decision.</summary>
public interface IRunCommand
{
}

/// <summary>Enter a node on the map.</summary>
public sealed class ChooseNodeCommand : IRunCommand
{
    public BaseNode Node { get; }

    public ChooseNodeCommand(BaseNode node)
    {
        Node = node;
    }

    public override string ToString() => $"ChooseNode({Node?.Type}, floor {Node?.Floor}, index {Node?.Index})";
}

/// <summary>Add one of the offered cards to the deck and move on.</summary>
public sealed class TakeRewardCommand : IRunCommand
{
    public CardData Card { get; }

    public TakeRewardCommand(CardData card)
    {
        Card = card;
    }

    public override string ToString() => $"TakeReward({Card?.CardName})";
}

/// <summary>Leave the offered cards and move on.</summary>
public sealed class SkipRewardCommand : IRunCommand
{
    public override string ToString() => "SkipReward";
}
