using System;
using System.Collections.Generic;

/// <summary>
/// What <see cref="CardSelectionCanvas"/> needs to show a card picker.
///
/// This is a view-side type. Combat itself asks for a choice with a
/// <see cref="CardChoiceDecision"/>; the scene turns that into one of these.
/// </summary>
public class CardSelectionRequest
{
    /// <summary>
    /// The cards the player may pick from. Must currently be in hand and have
    /// live CardViews registered with PlayerHandView.
    /// </summary>
    public IReadOnlyList<CardInstance> SourceCards;

    public int MinCount;
    public int MaxCount;
    public string PromptText;
    public string ConfirmLabel;
    public bool CanCancel;
    public Action<IReadOnlyList<CardInstance>> OnConfirm;
    public Action OnCancel;
}
