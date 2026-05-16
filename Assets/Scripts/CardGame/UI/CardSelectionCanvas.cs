using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardSelectionRequest
{
    /// <summary>
    /// The cards the player may pick from. Must currently be in hand and have
    /// live CardViews registered with PlayerHandView. Typically this is the
    /// hand minus the played card (when ExcludeSelf is true).
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

/// <summary>
/// In-hand selection canvas. The hand stays visible; a click on a hand card
/// during a prompt reparents that card's live view to <see cref="selectedTray"/>.
/// Clicking a tray card sends it back to its original hand slot.
/// </summary>
public class CardSelectionCanvas : MonoBehaviour
{
    public static CardSelectionCanvas Instance { get; private set; }

    [Header("Scene References")]
    [SerializeField] private PlayerHandView playerHandView;
    [Tooltip("Container under this canvas where selected card views are reparented to.")]
    [SerializeField] private Transform selectedTray;

    [Header("Canvas")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Texts / Buttons")]
    [SerializeField] private TMP_Text prompt;
    [SerializeField] private TMP_Text counter;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    [Header("Tuning")]
    [SerializeField] private float fadeTime = 0.2f;

    private struct OriginalSlot
    {
        public Transform Parent;
        public int SiblingIndex;
    }

    private readonly List<CardInstance> _selected = new List<CardInstance>();
    private readonly Dictionary<CardInstance, OriginalSlot> _origSlots =
        new Dictionary<CardInstance, OriginalSlot>();

    // Per-card (overrideSorting, sortingOrder) snapshot taken on Prompt() and
    // restored on Hide(). Both fields matter: in-hand cards live with
    // overrideSorting=false (cleared on pool return), so flipping it on is the
    // only way the lifted sortingOrder actually drives rendering.
    private struct CardSortingSnapshot
    {
        public bool OverrideSorting;
        public int SortingOrder;
    }
    private readonly Dictionary<CardInstance, CardSortingSnapshot> _preSelectionStates =
        new Dictionary<CardInstance, CardSortingSnapshot>();

    private CardSelectionRequest _req;
    private bool _isPrompting;
    private HashSet<CardInstance> _allowedSet;

    public bool IsPrompting => _isPrompting;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("CardSelectionCanvas: another instance already exists. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (confirmButton != null) confirmButton.onClick.AddListener(HandleConfirm);
        if (cancelButton != null) cancelButton.onClick.AddListener(HandleCancel);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;

        if (confirmButton != null) confirmButton.onClick.RemoveListener(HandleConfirm);
        if (cancelButton != null) cancelButton.onClick.RemoveListener(HandleCancel);

        if (canvasGroup != null) canvasGroup.DOKill();

        // Defensive: never leave selection mode latched on if the scene tears down mid-prompt.
        if (_isPrompting)
        {
            CardInteraction.OnHandCardClickedDuringSelection -= HandleHandCardClicked;
            CardInteraction.ExitSelectionMode();
        }
    }

    /// <summary>Show the prompt. Caller's coroutine should pause until OnConfirm or OnCancel fires.</summary>
    public void Prompt(CardSelectionRequest req)
    {
        if (_isPrompting)
        {
            Debug.LogWarning("CardSelectionCanvas: a prompt is already active. Ignoring new request.");
            return;
        }
        if (req == null)
        {
            Debug.LogWarning("CardSelectionCanvas: null request.");
            return;
        }
        if (req.SourceCards == null || req.SourceCards.Count == 0)
        {
            req.OnConfirm?.Invoke(Array.Empty<CardInstance>());
            return;
        }
        if (playerHandView == null)
        {
            Debug.LogError("CardSelectionCanvas: playerHandView is not assigned.");
            req.OnConfirm?.Invoke(Array.Empty<CardInstance>());
            return;
        }
        if (selectedTray == null)
        {
            Debug.LogError("CardSelectionCanvas: selectedTray is not assigned.");
            req.OnConfirm?.Invoke(Array.Empty<CardInstance>());
            return;
        }

        // Clamp counts to actual pool size.
        int max = Mathf.Clamp(req.MaxCount, 0, req.SourceCards.Count);
        int min = Mathf.Clamp(req.MinCount, 0, max);
        req.MinCount = min;
        req.MaxCount = max;

        _isPrompting = true;
        _req = req;
        _selected.Clear();
        _origSlots.Clear();
        _allowedSet = new HashSet<CardInstance>(req.SourceCards);

        if (prompt != null) prompt.text = req.PromptText ?? string.Empty;
        if (confirmButton != null)
        {
            var label = confirmButton.GetComponentInChildren<TMP_Text>();
            if (label != null && !string.IsNullOrEmpty(req.ConfirmLabel))
                label.text = req.ConfirmLabel;
        }
        if (cancelButton != null)
            cancelButton.gameObject.SetActive(req.CanCancel);

        UpdateCounter();
        UpdateConfirmInteractable();

        // Lift every live hand card above the selection canvas backdrop so they
        // remain visible AND clickable through the panel's blocksRaycasts. Each
        // CardVisual has its own override-sorting Canvas at base value 1, which
        // would otherwise be hidden by the panel sitting at 1000.
        LiftHandCardsForSelection();

        // Hand → selection mode + listen for clicks routed by CardInteraction.
        CardInteraction.EnterSelectionMode();
        CardInteraction.OnHandCardClickedDuringSelection += HandleHandCardClicked;

        Show();
    }

    private void LiftHandCardsForSelection()
    {
        _preSelectionStates.Clear();

        var activeViews = playerHandView.ActiveCardViews;
        if (activeViews == null) return;

        for (int i = 0; i < activeViews.Count; i++)
        {
            var view = activeViews[i];
            if (view == null || view.CardInstance == null) continue;

            _preSelectionStates[view.CardInstance] = new CardSortingSnapshot
            {
                OverrideSorting = view.CurrentOverrideSorting,
                SortingOrder = view.CurrentSortingOrder,
            };
            view.SetSortingState(true, CONSTANTS.CARD_HAND_DURING_SELECTION);
        }
    }

    private void RestoreHandCardSortingOrders(HashSet<CardInstance> exclude)
    {
        foreach (var kv in _preSelectionStates)
        {
            // Chosen cards are about to be animated out by Discard/ExhaustAction,
            // which sets overrideSorting=true and sortingOrder = CARD_ANIMATION_GHOST_START
            // (1500). Skip them here so we don't drop them to base for a couple of
            // frames between the canvas closing and the animation capturing them.
            if (exclude != null && exclude.Contains(kv.Key)) continue;

            if (playerHandView.TryGetView(kv.Key, out var view) && view != null)
            {
                view.SetSortingState(kv.Value.OverrideSorting, kv.Value.SortingOrder);
            }
        }
        _preSelectionStates.Clear();
    }

    private void HandleHandCardClicked(CardInstance ci)
    {
        if (!_isPrompting || _req == null || ci == null) return;

        // Excluded cards (e.g. the played card itself) shouldn't be pickable
        // even if a click somehow routes through.
        if (_allowedSet != null && !_allowedSet.Contains(ci)) return;

        if (_selected.Contains(ci))
        {
            DeselectCard(ci);
        }
        else
        {
            if (_selected.Count >= _req.MaxCount) return;
            SelectCard(ci);
        }

        UpdateCounter();
        UpdateConfirmInteractable();
    }

    private void SelectCard(CardInstance ci)
    {
        if (!playerHandView.TryGetView(ci, out var view) || view == null) return;

        Transform origParent = view.transform.parent;
        int origSiblingIndex = view.transform.GetSiblingIndex();
        _origSlots[ci] = new OriginalSlot
        {
            Parent = origParent,
            SiblingIndex = origSiblingIndex,
        };

        // Reparent into the tray. worldPositionStays=false so the layout group
        // on the tray (if any) places the card cleanly.
        view.transform.SetParent(selectedTray, false);

        // Force overrideSorting=true and bump above the lifted-hand band so the
        // tray card actually sits on top of the rest of the hand and the panel.
        view.SetSortingState(true, CONSTANTS.CARD_SELECTION_TRAY_SORTING_ORDER);

        _selected.Add(ci);
    }

    private void DeselectCard(CardInstance ci)
    {
        if (!_origSlots.TryGetValue(ci, out var slot))
        {
            _selected.Remove(ci);
            return;
        }

        if (playerHandView.TryGetView(ci, out var view) && view != null && slot.Parent != null)
        {
            view.transform.SetParent(slot.Parent, false);
            // Other hand siblings may have shifted while this card was in the tray;
            // clamp the restored sibling index to be safe.
            int clamped = Mathf.Clamp(slot.SiblingIndex, 0, slot.Parent.childCount - 1);
            view.transform.SetSiblingIndex(clamped);
            // Drop back to the lifted-hand band (override on, sortingOrder=1050)
            // so the card is still above the selection canvas backdrop.
            view.SetSortingState(true, CONSTANTS.CARD_HAND_DURING_SELECTION);
        }
        _origSlots.Remove(ci);
        _selected.Remove(ci);
    }

    private void UpdateCounter()
    {
        if (counter == null || _req == null) return;
        counter.text = $"{_selected.Count} / {_req.MaxCount}";
    }

    private void UpdateConfirmInteractable()
    {
        if (confirmButton == null || _req == null) return;
        confirmButton.interactable = _selected.Count >= _req.MinCount;
    }

    private void HandleConfirm()
    {
        if (!_isPrompting || _req == null) return;
        if (_selected.Count < _req.MinCount) return;

        // Move chosen views back to their original parent BEFORE this canvas fades.
        // worldPositionStays=true keeps them visually in place so there's no jump
        // between confirm and the start of the DiscardAction's fly-out animation.
        // It also prevents the canvasGroup's alpha=0 fade from making the soon-to-be-
        // animated cards invisible while the discard animation runs.
        foreach (var ci in _selected)
        {
            if (!_origSlots.TryGetValue(ci, out var slot)) continue;
            if (slot.Parent == null) continue;
            if (!playerHandView.TryGetView(ci, out var view) || view == null) continue;

            view.transform.SetParent(slot.Parent, true);
        }

        var snapshot = new List<CardInstance>(_selected);
        var req = _req;
        Hide(() => req.OnConfirm?.Invoke(snapshot));
    }

    private void HandleCancel()
    {
        if (!_isPrompting || _req == null || !_req.CanCancel) return;

        // Put everything back into the hand layout slots.
        foreach (var ci in new List<CardInstance>(_selected))
        {
            DeselectCard(ci);
        }

        var req = _req;
        Hide(() => req.OnCancel?.Invoke());
    }

    private void Show()
    {
        if (canvasGroup == null) return;
        canvasGroup.DOKill();
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.DOFade(1f, fadeTime);
    }

    private void Hide(Action onComplete)
    {
        // Snapshot chosen BEFORE clearing state so RestoreHandCardSortingOrders
        // can skip them — their about-to-run animation owns their sortingOrder.
        var chosenSnapshot = new HashSet<CardInstance>(_selected);

        // Tear down state immediately so a second prompt can't sneak in during the fade,
        // and so hand cards regain their normal click behavior right away.
        _isPrompting = false;
        _req = null;
        _allowedSet = null;
        _selected.Clear();
        _origSlots.Clear();

        CardInteraction.OnHandCardClickedDuringSelection -= HandleHandCardClicked;
        CardInteraction.ExitSelectionMode();

        if (canvasGroup != null)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.DOKill();
            canvasGroup.DOFade(0f, fadeTime).OnComplete(() =>
            {
                RestoreHandCardSortingOrders(chosenSnapshot);
                onComplete?.Invoke();
            });
        }
        else
        {
            RestoreHandCardSortingOrders(chosenSnapshot);
            onComplete?.Invoke();
        }
    }
}
