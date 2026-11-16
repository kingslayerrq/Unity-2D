using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// The scene side of a combat. The combat itself belongs to the run; this
/// connects it to what is on screen: unit views, the hand, the End Turn
/// button, the card selection canvas, and the runner that paces the action
/// queue to the animations.
///
/// It holds no rules of its own. A combat runs the same without it.
/// </summary>
public class CombatPresenter : MonoBehaviour
{
    [FormerlySerializedAs("unitsManager")]
    [SerializeField] private UnitSpawner unitSpawner;

    [Header("Canvas UI")]
    [SerializeField] private PlayerHandView playerHandView;
    [SerializeField] private EndTurnButton endTurnButton;
    [SerializeField] private UnitEnergyView unitEnergyView;

    // The combat this presenter attached to in Awake. Kept so it can detach
    // without asking for the run again while the game is shutting down.
    private CombatManager boundCombat;
    private CombatQueueRunner queueRunner;

    /// <summary>The combat this presenter shows: the current run's.</summary>
    public CombatManager Combat => boundCombat ?? RunManager.Instance.Run.Combat;

    public PlayerHandView PlayerHandView => playerHandView;

    private void Awake()
    {
        RunManager.Instance.RegisterCombatPresenter(this);

        boundCombat = RunManager.Instance.Run.Combat;

        queueRunner = new CombatQueueRunner(boundCombat.ActionQueue, ResolveCoroutineHost);
        if (playerHandView != null) queueRunner.AddPresenter(playerHandView);

        boundCombat.DecisionRequested += PresentDecision;

        if (unitSpawner == null)
        {
            Debug.LogError("Unit spawner isn't set", this);
        }
    }

    /// <summary>
    /// Puts a battle on screen. Called once its units exist and before it
    /// begins, so the views receive the opening health and energy.
    /// </summary>
    public void ShowBattle(CombatManager combat)
    {
        if (unitSpawner != null) unitSpawner.SpawnUnitViews(combat);

        if (endTurnButton != null) endTurnButton.Initialize(combat.Player);
        if (unitEnergyView != null) unitEnergyView.Initialize(combat.Player);
    }

    private MonoBehaviour ResolveCoroutineHost()
    {
        // The persistent runner keeps going when the combat panel is hidden.
        if (CoroutineRunner.Instance != null) return CoroutineRunner.Instance;
        return this;
    }

    /// <summary>
    /// The person's way of answering a decision: a card choice opens the
    /// selection canvas, whose buttons submit the same command a bot would.
    /// The player's turn needs nothing here; the hand and End Turn button
    /// are already on screen.
    /// </summary>
    private void PresentDecision(ICombatDecision decision)
    {
        if (decision is not CardChoiceDecision choice) return;

        if (CardSelectionCanvas.Instance == null)
        {
            Debug.LogError("CardSelectionCanvas.Instance is null. " +
                           "Make sure the CardSelectionCanvas component is present on the combat scene's canvas.");
            Combat.Submit(new ChooseCardsCommand(choice.GetMinimalPicks()));
            return;
        }

        string verb = choice.Kind == CardChoiceKind.Exhaust ? "Exhaust" : "Discard";

        CardSelectionCanvas.Instance.Prompt(new CardSelectionRequest
        {
            SourceCards  = choice.Options,
            MinCount     = choice.MinCount,
            MaxCount     = choice.MaxCount,
            PromptText   = $"{verb} {choice.MaxCount} card(s)",
            ConfirmLabel = verb,
            CanCancel    = choice.IsOptional,
            OnConfirm    = picks => SubmitChoiceFromScreen(choice, picks),
            OnCancel     = () => SubmitChoiceFromScreen(choice, Array.Empty<CardInstance>()),
        });
    }

    private void SubmitChoiceFromScreen(CardChoiceDecision choice, IReadOnlyList<CardInstance> picks)
    {
        CommandResult result = Combat.Submit(new ChooseCardsCommand(picks));
        if (result.Accepted) return;

        // The canvas reports an empty pick when it cannot open. Combat must
        // not stay suspended on a choice nobody can make.
        Debug.LogWarning($"Card choice from the screen was rejected ({result.Reason}). " +
                         "Using the smallest valid choice instead.");
        Combat.Submit(new ChooseCardsCommand(choice.GetMinimalPicks()));
    }

    private void OnDestroy()
    {
        queueRunner?.Stop();

        if (boundCombat != null)
        {
            boundCombat.DecisionRequested -= PresentDecision;
        }
    }
}
