using System;
using UnityEngine;

[Serializable]
public struct DynamicCardInstantiateRecipe
{
    [Tooltip("The base ScriptableObject data for the card to generate.")]
    public CardData BaseCard;

    [Tooltip("If true, the generated card will be upgraded IF the card that cast this effect is upgraded.")]
    public bool InheritUpgradeState;

    [Tooltip("If true, this card will ALWAYS be generated as an upgraded version (e.g., an effect that explicitly says 'Shuffle an Upgraded Smite').")]
    public bool AlwaysSpawnUpgraded;
}
