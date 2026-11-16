using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Picks the cards offered after a combat: which pool to draw from, the
/// rarity of each card, and the rare-card pity counter that carries through
/// a run. Plain C#; the reward screen only shows what this returns.
/// </summary>
public sealed class CardRewardGenerator
{
    private const int StartingRareChanceOffset = -5;

    private readonly Dictionary<CardClass, List<CardData>> poolsByClass =
        new Dictionary<CardClass, List<CardData>>();

    // Every pool merged, built on first use after the pools change.
    private List<CardData> combinedPool;

    // Rises with each common card offered and resets on a rare one.
    private int rareChanceOffset = StartingRareChanceOffset;

    /// <summary>
    /// Replaces the pools rewards are drawn from. The pity counter and any
    /// reward already on offer are not affected.
    /// </summary>
    public void SetPools(IReadOnlyDictionary<CardClass, List<CardData>> pools)
    {
        poolsByClass.Clear();
        combinedPool = null;

        if (pools == null) return;

        foreach (KeyValuePair<CardClass, List<CardData>> pool in pools)
        {
            poolsByClass[pool.Key] = pool.Value == null
                ? new List<CardData>()
                : new List<CardData>(pool.Value);
        }
    }

    /// <summary>
    /// Offers up to <paramref name="amount"/> different cards.
    /// </summary>
    /// <param name="allowClassOverride">Draw from every class's pool, not only the player's.</param>
    /// <param name="rng">The run's loot stream, so a seed reproduces the offer.</param>
    public List<CardData> Generate(
        CardClass cardClass,
        int amount,
        bool isElite,
        bool allowClassOverride,
        System.Random rng)
    {
        List<CardData> rewardPool;

        if (allowClassOverride)
        {
            if (combinedPool == null)
            {
                combinedPool = new List<CardData>();
                foreach (List<CardData> pool in poolsByClass.Values)
                {
                    combinedPool.AddRange(pool);
                }
            }

            rewardPool = combinedPool;
        }
        else if (!poolsByClass.TryGetValue(cardClass, out rewardPool))
        {
            Debug.LogError($"Reward pool for class {cardClass} not found");
            return new List<CardData>();
        }

        return GenerateFromPool(rewardPool, amount, isElite, rng);
    }

    private List<CardData> GenerateFromPool(
        List<CardData> pool,
        int amount,
        bool isElite,
        System.Random rng)
    {
        List<CardData> selectedCards = new List<CardData>();
        // Copy the pool so we can remove cards and prevent duplicates
        List<CardData> availablePool = new List<CardData>(pool);

        int baseRareChance = isElite ? 10 : 3;
        int baseUncommonChance = isElite ? 40 : 37;

        for (int i = 0; i < amount; i++)
        {
            // Nothing left to offer.
            if (availablePool.Count == 0) break;

            int actualRareChance = Mathf.Clamp(baseRareChance + rareChanceOffset, 0, 100);

            int roll = rng.Next(0, 100);
            CardRarity rarity;

            if (roll < actualRareChance)
            {
                rarity = CardRarity.Rare;
                // reset pity
                rareChanceOffset = StartingRareChanceOffset;
            }
            else if (roll < actualRareChance + baseUncommonChance)
            {
                rarity = CardRarity.Uncommon;
            }
            else
            {
                rarity = CardRarity.Common;
                rareChanceOffset += 1;
            }

            List<CardData> cardsOfRarity = availablePool.FindAll(card => card.CardRarity == rarity);

            if (cardsOfRarity.Count == 0)
            {
                Debug.LogWarning($"Ran out of {rarity} cards! Falling back to general pool.");
                cardsOfRarity = availablePool;
            }

            CardData chosenCard = cardsOfRarity[rng.Next(cardsOfRarity.Count)];

            selectedCards.Add(chosenCard);
            availablePool.Remove(chosenCard);
        }

        return selectedCards;
    }
}
