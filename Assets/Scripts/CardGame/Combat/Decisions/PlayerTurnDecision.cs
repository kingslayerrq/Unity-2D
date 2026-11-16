using System.Collections.Generic;

/// <summary>
/// It is the player's action phase: they may play a card or end the turn.
/// Answered with a <see cref="PlayCardCommand"/> or an <see cref="EndTurnCommand"/>.
///
/// The same decision stays pending for the whole phase. What is playable
/// changes as energy is spent, so ask again after each card.
/// </summary>
public sealed class PlayerTurnDecision : ICombatDecision
{
    private readonly CombatManager _combat;

    public BasePlayerUnitInstance Player { get; }
    public int Turn { get; }

    public PlayerTurnDecision(CombatManager combat, BasePlayerUnitInstance player, int turn)
    {
        _combat = combat;
        Player = player;
        Turn = turn;
    }

    /// <summary>
    /// True when the card can be paid for and has something legal to be
    /// played at.
    /// </summary>
    public bool CanPlay(CardInstance card)
    {
        return _combat.CanPlayCard(card) && _combat.GetTargetOptions(card).Count > 0;
    }

    /// <summary>The cards in hand that can be played right now.</summary>
    public List<CardInstance> GetPlayableCards()
    {
        List<CardInstance> playable = new List<CardInstance>();
        IReadOnlyList<CardInstance> hand = Player.Hand.Cards;

        for (int i = 0; i < hand.Count; i++)
        {
            if (CanPlay(hand[i])) playable.Add(hand[i]);
        }

        return playable;
    }

    /// <summary>
    /// Every legal way to target the card. Each entry is the full list of
    /// units one play would be aimed at: one entry per enemy for a card that
    /// hits a single enemy, a single entry for a card that hits them all.
    /// </summary>
    public List<IReadOnlyList<BaseUnitInstance>> GetTargetOptions(CardInstance card)
    {
        return _combat.GetTargetOptions(card);
    }

    /// <summary>
    /// Every legal card play right now, as commands ready to submit. Ending
    /// the turn is always legal as well.
    /// </summary>
    public List<PlayCardCommand> GetLegalPlays()
    {
        List<PlayCardCommand> plays = new List<PlayCardCommand>();
        IReadOnlyList<CardInstance> hand = Player.Hand.Cards;

        for (int i = 0; i < hand.Count; i++)
        {
            CardInstance card = hand[i];
            if (!_combat.CanPlayCard(card)) continue;

            List<IReadOnlyList<BaseUnitInstance>> options = _combat.GetTargetOptions(card);
            for (int j = 0; j < options.Count; j++)
            {
                plays.Add(new PlayCardCommand(card, options[j]));
            }
        }

        return plays;
    }

    public override string ToString() => $"PlayerTurn(turn {Turn})";
}
