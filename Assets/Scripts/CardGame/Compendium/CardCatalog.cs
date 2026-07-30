using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds all cards definition
/// </summary>
[CreateAssetMenu(menuName = "Cards/Card Catalog", fileName =  "CardCatalog")]
public sealed class CardCatalog : ScriptableObject
{
    [SerializeField] private List<CardData> cards = new();

    private Dictionary<string, CardData> cardsById;

    public IReadOnlyList<CardData> Cards => cards;

    public bool TryGetCard(string id, out CardData card)
    {
        EnsureLookupCreated();
        return cardsById.TryGetValue(id, out card);
    }

    private void EnsureLookupCreated()
    {
        if (cardsById != null)
            return;

        cardsById = new Dictionary<string, CardData>();

        foreach (CardData card in cards)
        {
            if (card == null || string.IsNullOrWhiteSpace(card.ID))
                continue;

            if (!cardsById.TryAdd(card.ID, card))
            {
                Debug.LogError(
                    $"Duplicate card ID '{card.ID}' found in catalog.",
                    this);
            }
        }
    }

#if UNITY_EDITOR
    private const string CardDataRoot = "Assets/Data/CardData";

    [ContextMenu("Refresh Catalog From CardData")]
    public void RefreshFromCardData()
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets(
            $"t:{nameof(CardData)}",
            new[] { CardDataRoot });

        List<string> assetPaths = new(guids.Length);

        foreach (string guid in guids)
        {
            assetPaths.Add(
                UnityEditor.AssetDatabase.GUIDToAssetPath(guid));
        }

        assetPaths.Sort(System.StringComparer.Ordinal);

        List<CardData> foundCards = new(assetPaths.Count);

        foreach (string assetPath in assetPaths)
        {
            CardData card =
                UnityEditor.AssetDatabase.LoadAssetAtPath<CardData>(
                    assetPath);

            if (card != null)
                foundCards.Add(card);
        }

        UnityEditor.Undo.RecordObject(
            this,
            "Refresh Card Catalog");

        cards = foundCards;
        cardsById = null;

        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.AssetDatabase.SaveAssets();

        Debug.Log(
            $"Loaded {cards.Count} cards from '{CardDataRoot}'.",
            this);
    }

    private void OnValidate()
    {
        cardsById = null;

        HashSet<string> ids = new();

        foreach (CardData card in cards)
        {
            if (card == null)
                continue;

            if (string.IsNullOrWhiteSpace(card.ID))
            {
                Debug.LogError(
                    $"Card '{card.name}' has no persistent ID.",
                    card);

                continue;
            }

            if (!ids.Add(card.ID))
            {
                Debug.LogError(
                    $"Duplicate card ID '{card.ID}' on '{card.name}'.",
                    card);
            }
        }
    }
#endif
}
