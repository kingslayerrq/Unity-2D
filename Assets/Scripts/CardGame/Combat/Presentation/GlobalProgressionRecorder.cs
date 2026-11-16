using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Passes a run's progression reports on to the game's
/// <see cref="GlobalProgressionManager"/>, when there is one. This is the only
/// place the run and combat rules meet the progression singleton.
/// </summary>
public sealed class GlobalProgressionRecorder : IRunProgressRecorder
{
    public void RecordCardPlayed(CardInstance card)
    {
        if (GlobalProgressionManager.HasInstance)
        {
            GlobalProgressionManager.Instance.RecordCardPlayed(card);
        }
    }

    public void RecordCardsOffered(IReadOnlyList<CardData> cards)
    {
        if (!GlobalProgressionManager.HasInstance || cards == null) return;

        GlobalProgressionManager progression = GlobalProgressionManager.Instance;

        // A card counts as discovered once it has been offered.
        for (int i = 0; i < cards.Count; i++)
        {
            progression.DiscoverCard(cards[i]);
        }

        progression.SaveCheckpoint();
    }

    public void RecordCardObtained(CardInstance card)
    {
        if (!GlobalProgressionManager.HasInstance) return;

        GlobalProgressionManager progression = GlobalProgressionManager.Instance;
        progression.RecordCardObtained(card);
        progression.SaveCheckpoint();
    }

    public void RecordNodeCompleted(PlayerUnitData character, BaseNode node)
    {
        if (GlobalProgressionManager.HasInstance)
        {
            GlobalProgressionManager.Instance.RecordNodeCompleted(character, node);
        }
    }

    public void RecordRunCompleted(PlayerUnitData character, bool won)
    {
        if (character == null)
        {
            Debug.LogError("Cannot record run completion without a character.");
            return;
        }

        if (!GlobalProgressionManager.HasInstance)
        {
            Debug.LogError("Cannot record run completion without global progression.");
            return;
        }

        GlobalProgressionManager.Instance.RecordRunCompleted(character, won);
    }
}
