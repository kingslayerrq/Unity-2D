using System;
using UnityEngine;

[Serializable]
public sealed class CharacterProgressRecord
{
    [SerializeField]
    private string characterId;

    [SerializeField]
    private CardClass cardClass;

    [SerializeField]
    private bool isUnlocked;

    [SerializeField]
    private int wins;

    [SerializeField]
    [UnityEngine.Serialization.FormerlySerializedAs("loss")]
    private int losses;

    [SerializeField]
    private int totalGames;

    public string CharacterId => characterId;
    public CardClass CardClass => cardClass;
    public bool IsUnlocked => isUnlocked;
    public int Wins => wins;
    public int Losses => losses;
    public int TotalGames => totalGames;

    public CharacterProgressRecord(
        string characterId,
        CardClass cardClass,
        bool unlockedByDefault)
    {
        this.characterId = characterId;
        this.cardClass = cardClass;
        isUnlocked = unlockedByDefault;
    }

    public bool Unlock()
    {
        if (isUnlocked)
            return false;

        isUnlocked = true;
        return true;
    }

    public void RecordGame(bool won)
    {
        totalGames++;

        if (won)
            wins++;
        else
            losses++;
    }

    public bool ReconcileIdentity(
        PlayerUnitData character)
    {
        if (character == null)
            return false;

        bool changed = false;

        if (string.IsNullOrWhiteSpace(characterId))
        {
            characterId = character.CharacterID;
            changed = true;
        }

        if (cardClass != character.PlayerCardClass)
        {
            cardClass = character.PlayerCardClass;
            changed = true;
        }

        if (character.IsUnlockedByDefault && !isUnlocked)
        {
            isUnlocked = true;
            changed = true;
        }

        return changed;
    }
}
