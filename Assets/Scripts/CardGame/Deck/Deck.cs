using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

[Serializable]
public class Deck
{
    [SerializeField] private DeckData startingDeckData;
    [SerializeField] private List<CardInstance> cardInstances = new List<CardInstance>();
    
    public int Count => cardInstances.Count;
    public bool IsEmpty => cardInstances.Count == 0;
    public IReadOnlyList<CardInstance> CardInstances => cardInstances;
    
    public event Action OnDeckChanged;

    public Deck(DeckData startingDeckData)
    {
        this.startingDeckData = startingDeckData;
    }

    public void InstantiateDeck(BasePlayerUnitInstance owner)
    {
        if (startingDeckData == null)
        {
            Debug.LogWarning("Deck data is null");
            return;
        }
        cardInstances.Clear();
        foreach (CardData cd in startingDeckData.CardDataList)
        {
            cardInstances.Add(new CardInstance(cd, owner));
        }
        Debug.Log("Instantiated deck");
    }

    public void AddCardToDeck(CardInstance cardInstance)
    {
        if (cardInstance == null) return;
        cardInstances.Add(cardInstance);
        OnDeckChanged?.Invoke();
        Debug.Log($"Added {cardInstance.CurrentName} to deck");
    }
    
    public int CountStrikeCardsPossessed()
    {
        if (cardInstances == null || cardInstances.Count == 0) return 0;

        return cardInstances.Where(ci => ci.CurrentName.Contains("Strike")).ToList().Count;
    }
}