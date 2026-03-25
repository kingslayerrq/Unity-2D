using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Deck", menuName = "Cards/Deck Data")]
public class DeckData : ScriptableObject
{
    [SerializeField] private List<CardData> cardDataList = new List<CardData>();
    public List<CardData> CardDataList => cardDataList;
}
