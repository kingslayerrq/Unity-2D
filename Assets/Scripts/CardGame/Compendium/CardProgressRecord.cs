using System;
using UnityEngine;

[Serializable]
public sealed class CardProgressRecord
{
    [SerializeField] private string cardId;

    [SerializeField] private bool isDiscovered;
    [SerializeField] private bool isUnlocked;

    [SerializeField] private int timesObtained;
    [SerializeField] private int timesPlayed;
    [SerializeField] private int runsWonWithCard;

    public string CardId => cardId;

    public bool IsDiscovered => isDiscovered;
    public bool IsUnlocked => isUnlocked;

    public int TimesObtained => timesObtained;
    public int TimesPlayed => timesPlayed;
    public int RunsWonWithCard => runsWonWithCard;

    public CardProgressRecord(
        string cardId,
        bool unlockedByDefault)
    {
        this.cardId = cardId;

        isUnlocked = unlockedByDefault;
        isDiscovered = unlockedByDefault;
    }

    public bool Discover()
    {
        if (isDiscovered)
            return false;

        isDiscovered = true;
        return true;
    }

    public bool Unlock()
    {
        bool changed = !isUnlocked || !isDiscovered;

        isUnlocked = true;
        isDiscovered = true;

        return changed;
    }

    public bool RecordObtained()
    {
        isDiscovered = true;
        timesObtained++;

        return true;
    }

    public bool RecordPlayed()
    {
        isDiscovered = true;
        timesPlayed++;

        return true;
    }

    public bool RecordRunWon()
    {
        runsWonWithCard++;
        return true;
    }

    public bool ReconcileDefaultUnlock(bool unlockedByDefault)
    {
        if (!unlockedByDefault ||
            (isUnlocked && isDiscovered))
        {
            return false;
        }

        isUnlocked = true;
        isDiscovered = true;
        return true;
    }
}
