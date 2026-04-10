using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CombatRewardManager : MonoBehaviour
{
    [SerializeField] private CardRewardData ironcladReward;
    [SerializeField] private CardRewardData silentReward;
    [SerializeField] private CardRewardData necroReward;
    [SerializeField] private CardRewardData regentReward;
    [SerializeField] private CardRewardData neutralReward;
    
    private List<CardData> cachedOverridePool = null;

    private int rareChanceOffset = -5;
    
    private Dictionary<CardClass, List<CardData>> cardRewardDict = new Dictionary<CardClass, List<CardData>>();

    public void Initialize()
    {
        cardRewardDict.Clear();
        cardRewardDict.Add(CardClass.Ironclad, ironcladReward.availableCards.Where(cd => cd.IsUnlocked).ToList());
        cardRewardDict.Add(CardClass.Silent, silentReward.availableCards.Where(cd => cd.IsUnlocked).ToList());
        cardRewardDict.Add(CardClass.Necro, necroReward.availableCards.Where(cd => cd.IsUnlocked).ToList());
        cardRewardDict.Add(CardClass.Regent, regentReward.availableCards.Where(cd => cd.IsUnlocked).ToList());
        cardRewardDict.Add(CardClass.None, neutralReward.availableCards.Where(cd => cd.IsUnlocked).ToList());

        cachedOverridePool = null;
    }

    public List<CardData> GetCardReward(CardClass cardClass, int amount, bool isElite, 
        bool allowClassOverride = false)
    {
        List<CardData> rewardPool;

        if (allowClassOverride)
        {
            if (cachedOverridePool == null)
            {
                cachedOverridePool = new List<CardData>();
                foreach (var pool in cardRewardDict.Values)
                {
                    cachedOverridePool.AddRange(pool);
                }
            }
            rewardPool = cachedOverridePool;
        }
        else
        {
            if (cardRewardDict.TryGetValue(cardClass, out List<CardData> pool))
            {
                rewardPool = pool;
            }
            else
            {
                Debug.LogError($"Reward pool for class {cardClass} not found");
                return new List<CardData>();
            }
        }
        return GenerateCardRewardsFromPool(rewardPool, amount, isElite);
    }

   

    private List<CardData> GenerateCardRewardsFromPool(List<CardData> pool, int amount, bool isElite)
    {
        List<CardData> selectedCards = new List<CardData>();
        // Copy the pool so we can remove cards and prevent duplicates
        List<CardData> availablePool = new List<CardData>(pool);
        
        int baseRareChance = isElite ? 10 : 3;
        int baseUncommonChance = isElite ? 40 : 37;

        for (int i = 0; i < amount; i++)
        {
            int actualRareChance = baseRareChance + rareChanceOffset;
            actualRareChance = Mathf.Clamp(actualRareChance, 0, 100);

            int roll = Random.Range(0, 100);
            CardRarity rarity;

            if (roll < actualRareChance)
            {
                rarity = CardRarity.Rare;
                // reset pity
                rareChanceOffset = -5;
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
            
            List<CardData> cardsOfRarity = availablePool.Where(cd => cd.CardRarity == rarity).ToList();

            if (cardsOfRarity.Count == 0)
            {
                Debug.LogWarning($"Ran out of {rarity} cards! Falling back to general pool.");
                cardsOfRarity = availablePool;
            }
            
            int randomInd = Random.Range(0, cardsOfRarity.Count);
            CardData chosenCard = cardsOfRarity[randomInd];
            
            selectedCards.Add(chosenCard);
            
            if (!chosenCard.IsDiscovered) chosenCard.SetDiscovered(true);
            
            availablePool.Remove(chosenCard);
        }
        return selectedCards;
    }

    private CardData GetWeightedRandomCard(List<CardData> pool)
    {
        int totalWeight = 0;
        foreach (var cd in pool)
        {
            totalWeight += cd.SpawnWeight;
        }
        
        int randValue = Random.Range(0, totalWeight);
        int currentWeight = 0;
        foreach (var cd in pool)
        {
            currentWeight += cd.SpawnWeight;
            if (randValue < currentWeight)
            {
                return cd;
            }
        }
        Debug.LogWarning("Weighted random fallback triggered. Check your card weights.");
        return pool[0];
    }
}
