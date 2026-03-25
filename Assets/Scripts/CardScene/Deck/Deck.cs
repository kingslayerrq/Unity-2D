using System;
using System.Collections.Generic;
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

    public Deck(DeckData startingDeckData)
    {
        this.startingDeckData = startingDeckData;
    }

    public void InstantiateDeck()
    {
        if (startingDeckData == null)
        {
            Debug.LogWarning("Deck data is null");
            return;
        }
        cardInstances.Clear();
        foreach (CardData cd in startingDeckData.CardDataList)
        {
            cardInstances.Add(new CardInstance(cd));
        }
        Debug.Log("Instantiated deck");
    }
    
}