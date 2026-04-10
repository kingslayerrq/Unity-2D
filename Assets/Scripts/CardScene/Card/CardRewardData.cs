using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Card Reward Data", menuName = "Cards/CardRewardData")]
public class CardRewardData : ScriptableObject
{
    public List<CardData> availableCards;
    
    
}
