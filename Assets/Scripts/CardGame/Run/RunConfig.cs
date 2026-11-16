using System.Collections.Generic;

/// <summary>
/// Receives what happens in a run that should count towards long-term
/// progression. A run reports to it and never looks progression up itself.
/// </summary>
public interface IRunProgressRecorder : ICombatProgressRecorder
{
    /// <summary>These cards were put in front of the player as a reward.</summary>
    void RecordCardsOffered(IReadOnlyList<CardData> cards);

    /// <summary>The player added this card to their deck.</summary>
    void RecordCardObtained(CardInstance card);

    void RecordNodeCompleted(PlayerUnitData character, BaseNode node);
    void RecordRunCompleted(PlayerUnitData character, bool won);
}

/// <summary>
/// The settings a run is played with. In the game some of these are known
/// only once the run scene has loaded, so they may be filled in after the
/// run has been created, as long as that happens before the map is needed.
/// </summary>
public sealed class RunConfig
{
    /// <summary>The size of the map to build.</summary>
    public MapSettings Map;

    /// <summary>The node contents the map and Unknown nodes draw from.</summary>
    public NodeConfigLibrary NodeLibrary;

    /// <summary>How many cards are offered after a combat.</summary>
    public int CardRewardAmount = 3;
}
