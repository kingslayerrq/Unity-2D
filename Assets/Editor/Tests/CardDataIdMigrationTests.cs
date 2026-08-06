using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class CardDataIdMigrationTests
{
    [Test]
    public void AssignMissingIdUsesProvidedAssetGuid()
    {
        CardData card = ScriptableObject.CreateInstance<CardData>();

        try
        {
            SerializedObject serializedCard = new(card);

            bool assigned = CardDataIdMigration.AssignMissingId(
                serializedCard,
                "new-card-guid");

            Assert.That(assigned, Is.True);
            Assert.That(card.ID, Is.EqualTo("new-card-guid"));
        }
        finally
        {
            Object.DestroyImmediate(card);
        }
    }

    [Test]
    public void AssignMissingIdPreservesExistingId()
    {
        CardData card = ScriptableObject.CreateInstance<CardData>();

        try
        {
            SerializedObject serializedCard = new(card);

            CardDataIdMigration.AssignMissingId(
                serializedCard,
                "existing-card-id");

            bool assigned = CardDataIdMigration.AssignMissingId(
                serializedCard,
                "replacement-card-id");

            Assert.That(assigned, Is.False);
            Assert.That(card.ID, Is.EqualTo("existing-card-id"));
        }
        finally
        {
            Object.DestroyImmediate(card);
        }
    }
}
