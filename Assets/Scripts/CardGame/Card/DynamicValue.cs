using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public struct DynamicValue
{
    [Tooltip("The guaranteed base amount. For purely static cards, just use this.")]
    public int baseValue; 
    
    [Header("Dynamic Bonus (Optional)")]
    [Tooltip("Where to get the additive bonus value from.")]
    public ValueSource bonusSource;
    
    [Tooltip("Multiplies the bonus amount (e.g., x3 per Strike card).")]
    public int bonusMultiplier;

    public int Resolve(CardContext context)
    {
        int bonusValue = 0;
        switch (bonusSource)
        {
            case ValueSource.Static:
                bonusValue = 0;
                break;
            case ValueSource.CurrentBlock:
                bonusValue = context.Source.CurrentBlock;
                break;
            case ValueSource.StrikeCardsPossessed:
                if (context.Source is BasePlayerUnitInstance player)
                {
                    bonusValue = player.DeckInstance.CountStrikeCardsPossessed();
                }
                else
                {
                    Debug.LogError($"StrikeCardsPossessed is not applicable to a non BasePlayerUnitInstance.");
                }
                break;
            default:
                Debug.LogWarning($"ValueSource {bonusSource} not handled!");
                break;
        }

        
        return baseValue + bonusValue *  bonusMultiplier;
    }
}

public enum ValueSource
{
    Static,
    CurrentBlock,
    StrikeCardsPossessed,
}