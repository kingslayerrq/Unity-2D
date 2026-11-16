using System.Collections.Generic;

/// <summary>
/// The listed cards are about to leave the player's hand (played, discarded
/// or exhausted). Published before the hand changes.
/// </summary>
public sealed class CardsLeavingHandEvent : ICombatEvent
{
    public BasePlayerUnitInstance Player { get; }
    public IReadOnlyList<CardInstance> Cards { get; }

    public CardsLeavingHandEvent(BasePlayerUnitInstance player, IReadOnlyList<CardInstance> cards)
    {
        Player = player;
        Cards = cards;
    }

    public override string ToString() => $"CardsLeavingHand({Cards?.Count ?? 0})";
}

/// <summary>
/// A played card has finished resolving: the effects it queued have run and
/// the card sits wherever it ended up (see <see cref="CardInstance.CurrentCardState"/>).
/// </summary>
public sealed class CardPlayResolvedEvent : ICombatEvent
{
    public BasePlayerUnitInstance Player { get; }
    public CardInstance Card { get; }

    public CardPlayResolvedEvent(BasePlayerUnitInstance player, CardInstance card)
    {
        Player = player;
        Card = card;
    }

    public override string ToString() => $"CardPlayResolved({Card?.CurrentName})";
}

/// <summary>The listed cards were discarded from the player's hand.</summary>
public sealed class CardsDiscardedEvent : ICombatEvent
{
    public BasePlayerUnitInstance Player { get; }
    public IReadOnlyList<CardInstance> Cards { get; }
    public CardMoveCause Cause { get; }

    public CardsDiscardedEvent(BasePlayerUnitInstance player, IReadOnlyList<CardInstance> cards,
        CardMoveCause cause)
    {
        Player = player;
        Cards = cards;
        Cause = cause;
    }

    public override string ToString() => $"CardsDiscarded({Cards?.Count ?? 0}, {Cause})";
}

/// <summary>A card was exhausted from the player's hand by an effect.</summary>
public sealed class CardExhaustedEvent : ICombatEvent
{
    public BasePlayerUnitInstance Player { get; }
    public CardInstance Card { get; }

    public CardExhaustedEvent(BasePlayerUnitInstance player, CardInstance card)
    {
        Player = player;
        Card = card;
    }

    public override string ToString() => $"CardExhausted({Card?.CurrentName})";
}

/// <summary>
/// The player's hand should be shown as it is now. Published when cards have
/// arrived without a more specific event, such as the turn-start draw.
/// </summary>
public sealed class HandChangedEvent : ICombatEvent
{
    public BasePlayerUnitInstance Player { get; }

    public HandChangedEvent(BasePlayerUnitInstance player)
    {
        Player = player;
    }

    public override string ToString() => "HandChanged";
}
