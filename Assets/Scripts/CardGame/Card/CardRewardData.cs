using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
[CreateAssetMenu(fileName = "New Card Reward Data", menuName = "Cards/CardRewardData")]
public class CardRewardData : ScriptableObject
{
    [Header("Reward Filters")]
    [Tooltip("Check this if you want to filter by class.")]
    public bool filterByClass = true;
    public CardClass targetClass;
    
    [Tooltip("Check this if you want to filter by rarity.")]
    public bool filterByRarity = false;
    public CardRarity targetRarity;
    
    [Space(10)]
    public List<CardData> availableCards;
    
#if UNITY_EDITOR
    [ContextMenu("Auto-Populate Filtered Cards")]
    public void AutoPopulateCards()
    {
        availableCards ??= new List<CardData>();
        availableCards.Clear();

        // Find all assets of type CardData anywhere in the project
        string[] guids = AssetDatabase.FindAssets("t:CardData"); 

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            CardData card = AssetDatabase.LoadAssetAtPath<CardData>(path);
            
            if (card != null)
            {
                // 1. Check Class Filter
                if (filterByClass && card.CardClass != targetClass)
                {
                    continue; // Skip to the next card if it doesn't match
                }

                // 2. Check Rarity Filter
                if (filterByRarity && card.CardRarity != targetRarity)
                {
                    continue; // Skip to the next card if it doesn't match
                }

                // If it passes all active filters, add it to the pool!
                availableCards.Add(card);
            }
        }

        // Save the changes
        EditorUtility.SetDirty(this); 
        AssetDatabase.SaveAssets();
        
        Debug.Log($"Successfully loaded {availableCards.Count} cards matching your filters!");
    }
#endif
}
