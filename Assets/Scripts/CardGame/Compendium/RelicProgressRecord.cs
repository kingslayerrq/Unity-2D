using System;
using UnityEngine;

[Serializable]
public sealed class RelicProgressRecord
{
    [SerializeField] private string relicId;
    [SerializeField] private bool isUnlocked;
    [SerializeField] private bool isDiscovered;

    public string RelicId => relicId;
    public bool IsUnlocked => isUnlocked;
    public bool IsDiscovered => isDiscovered;

    public RelicProgressRecord(string relicId, bool isUnlockedByDefault, bool isDiscoveredByDefault)
    {
        this.relicId = relicId;
        isUnlocked = isUnlockedByDefault;
        
        // Can ONLY be discovered if it is also unlocked
        isDiscovered = isUnlockedByDefault && isDiscoveredByDefault;
    }

    /// <summary>
    /// Unlocks the relic so it can enter the loot pool.
    /// </summary>
    public bool Unlock()
    {
        if (isUnlocked)
            return false;

        isUnlocked = true;
        return true;
    }

    /// <summary>
    /// Marks the relic as seen/found during gameplay.
    /// Fails if the relic is currently locked.
    /// </summary>
    public bool Discover()
    {
        // Cannot discover a locked or already discovered relic
        if (!isUnlocked || isDiscovered)
            return false;

        isDiscovered = true;
        return true;
    }

    /// <summary>
    /// Synchronizes old save data with newly updated ScriptableObject defaults.
    /// </summary>
    public bool Reconcile(bool unlockedByDefault, bool discoveredByDefault)
    {
        bool modified = false;
        
        if (unlockedByDefault && !isUnlocked)
        {
            isUnlocked = true;
            modified = true;
        }
        
        if (isUnlocked && discoveredByDefault && !isDiscovered)
        {
            isDiscovered = true;
            modified = true;
        }

        return modified;
    }
}