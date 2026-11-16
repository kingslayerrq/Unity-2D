using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(CardView))]
public class CardInteraction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler,
    IEndDragHandler, IDragHandler, IPointerClickHandler
{
    private CardView _view;

    [Header("Interaction State")]
    private static bool isInteractingWithAnyCard = false;
    private ICardDropReceiver _currentReceiver = null;

    private bool isReleasedToPlay = false;
    private bool isSelected = false;
    private bool isDragging = false;
    private bool frameDelay = false;
    private bool _isDisplayOnly = false;

    // ----- Hand-card selection mode (for effects like SelectDiscard / SelectExhaust) -----
    // While active, left-clicking an in-hand card with an owner fires
    // OnHandCardClickedDuringSelection instead of picking the card up to play it,
    // and dragging is suppressed entirely.
    public static bool IsSelectionModeActive { get; private set; }
    public static event Action<CardInstance> OnHandCardClickedDuringSelection;

    public static void EnterSelectionMode() => IsSelectionModeActive = true;
    public static void ExitSelectionMode() => IsSelectionModeActive = false;
    
    [Header("Drag & Drop")]
    private int origSiblingIndex;
    private Transform origParent;
    private GameObject placeholder;

    public Action<CardInstance> OnCardSelectedToAdd;

    private void Awake()
    {
        _view = GetComponent<CardView>();
    }

    private void Update()
    {
        Mouse mouse = Mouse.current;

        if (mouse == null)
            return;

        Vector2 mousePosition = mouse.position.ReadValue();

        if (isSelected)
        {
            transform.position = mousePosition;
            if (frameDelay)
            {
                frameDelay = false;
                return;
            }
            UpdateReceivers(mousePosition);
        }
        
        if ((isSelected || isDragging) && mouse.leftButton.wasPressedThisFrame)
        {
            isSelected = false;
            isDragging = false;
            DropCard();
        }
    }

    private bool IsPlayCardAllowed()
    {
        if (_view.CardInstance == null || _view.CardInstance.CurrentOwner == null) return false;
        return _view.CardInstance.CurrentOwner.CombatManager.CurrentCombatState ==
               CombatManager.NodeState.PlayerAction;
        
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        // if (!IsPlayCardAllowed()) return;
        if (isInteractingWithAnyCard || isReleasedToPlay) return;
        _view.VisualHoverEnter();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isInteractingWithAnyCard || isReleasedToPlay) return;
        _view.VisualHoverExit();
    }

    public void SetDisplayOnly(bool displayOnly) => _isDisplayOnly = displayOnly;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_isDisplayOnly) return;
        if (IsSelectionModeActive) return;
        if (!IsPlayCardAllowed()) return;
        if (_view.CardInstance is not { CurrentCardState: CardState.InHand }) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (isSelected) return;

        if (!isInteractingWithAnyCard)
        {
            isDragging = true;
            PickUpCard();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            transform.position = eventData.position;
            UpdateReceivers(eventData.position);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            isDragging = false;
            ReleaseCard(eventData.position);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_isDisplayOnly) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (_view.CardInstance?.CurrentOwner == null)
        {
            // Try to select to add (used by NodeRewardManager card picker)
            OnCardSelectedToAdd?.Invoke(_view.CardInstance);
            return;
        }
        if (IsSelectionModeActive &&
            _view.CardInstance != null &&
            _view.CardInstance.CurrentCardState == CardState.InHand)
        {
            // Hand-card selection mode (Select Discard / Select Exhaust): a click
            // toggles the card's selection on the active CardSelectionCanvas
            // instead of picking it up to play.
            OnHandCardClickedDuringSelection?.Invoke(_view.CardInstance);
            return;
        }
        if (!IsPlayCardAllowed()) return;
        if (_view.CardInstance is not { CurrentCardState: CardState.InHand }) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (isDragging) return;

        if (!isInteractingWithAnyCard)
        {
            isSelected = true;
            frameDelay = true;
            PickUpCard();
        }
    }

    private void PickUpCard()
    {
        isInteractingWithAnyCard = true;
        _view.HideAllPopups();
        
        origParent = transform.parent;
        origSiblingIndex = transform.GetSiblingIndex();
        
        placeholder = new GameObject("CardPlaceholder");
        RectTransform rt = placeholder.AddComponent<RectTransform>();
        rt.SetParent(origParent);
        rt.SetSiblingIndex(origSiblingIndex);
        
        LayoutElement le = placeholder.AddComponent<LayoutElement>();
        le.preferredWidth = GetComponent<RectTransform>().rect.width;
        le.preferredHeight = GetComponent<RectTransform>().rect.height;
        
        transform.SetParent(origParent.parent);
        _view.SetSortingOrder(CONSTANTS.CARD_DRAG_SORTING_ORDER);
        _view.CanvasGroup.blocksRaycasts = false;
    }

    private void DropCard()
    {
        ClearHoveredReceiver();
        
        isInteractingWithAnyCard = false;
        isReleasedToPlay = false;
        isSelected = false;
        isDragging = false;
        frameDelay = false;
        _view.CanvasGroup.blocksRaycasts = true;

        if (placeholder != null) Destroy(placeholder);

        transform.SetParent(origParent);
        transform.SetSiblingIndex(origSiblingIndex);

        _view.VisualHoverExit(); // Handles resetting scale and base sorting
    }

    private void ReleaseCard(Vector2 screenPos)
    {
        UpdateReceivers(screenPos);
        isReleasedToPlay = true;
        
        bool isPlayed = TryPlayCard();
        
        ClearHoveredReceiver();

        if (!isPlayed) DropCard();
    }

    private bool TryPlayCard()
    {
        if (_currentReceiver == null) return false;
        CombatManager combatManager = _view.CardInstance.CurrentOwner.CombatManager;
        if (combatManager == null || !combatManager.CanPlayCard(_view.CardInstance)) return false;

        if (_currentReceiver.CanReceiveCard(_view.CardInstance.CurrentOwner, _view.CardInstance))
        {
            var targets = _currentReceiver.GetReceiverUnitInstance(_view.CardInstance.CurrentOwner, _view.CardInstance);
        
            // FIX: Pass 'true' so the reset doesn't ruin the card's visual state 
            // right before the DOTween animation captures it.
            ResetInteractionState(isBeingPlayed: true); 
        
            combatManager.PlayCard(_view.CardInstance, targets);
            return true;
        }
        return false;
    }

    private ICardDropReceiver FindReceiver(Vector2 screenPos)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current) { position = screenPos };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        
        ICardDropReceiver bestReceiver = null;
        int bestPriority = int.MinValue;
        bool hitPlayCardArea = false;
        ICardDropReceiver playCardArea = null;

        foreach (RaycastResult result in results)
        {
            ICardDropReceiver receiver = result.gameObject.GetComponentInParent<ICardDropReceiver>();
            if (!hitPlayCardArea && receiver is PlayCardArea)
            {
                hitPlayCardArea = true;
                playCardArea = receiver;
            }
            if (receiver == null) continue;
            if (!receiver.CanReceiveCard(_view.CardInstance.CurrentOwner, _view.CardInstance)) continue;

            if (bestReceiver == null || receiver.Priority > bestPriority)
            {
                bestReceiver = receiver;
                bestPriority = receiver.Priority;
            }
        }

        return bestReceiver ?? playCardArea;
    }

    private void UpdateReceivers(Vector2 screenPos)
    {
        ICardDropReceiver newReceiver = FindReceiver(screenPos);

        if (ReferenceEquals(newReceiver, _currentReceiver)) return;

        if (_currentReceiver != null)
        {
            _currentReceiver.OnHoverExit(_view.CardInstance.CurrentOwner, _view.CardInstance);
        }

        _currentReceiver = newReceiver;

        if (_currentReceiver != null)
        {
            if (!_view.CardInstance.CanBePlayed)
            {
                DropCard();
                _currentReceiver = null;
                return;
            }
            _currentReceiver.OnHoverEnter(_view.CardInstance.CurrentOwner, _view.CardInstance);
        }
    }

    private void ClearHoveredReceiver()
    {
        if (_currentReceiver != null)
        {
            _currentReceiver.OnHoverExit(_view.CardInstance.CurrentOwner, _view.CardInstance);
            _currentReceiver = null;
        }
    }

    public void ResetInteractionState(bool isBeingPlayed = false) 
    {
        ClearHoveredReceiver();
        isInteractingWithAnyCard = false;
        isSelected = false;
        isDragging = false;
        frameDelay = false;

        if (placeholder != null)
        {
            Destroy(placeholder);
            placeholder = null;
        }

        origParent = null;
        origSiblingIndex = -1;

        // FIX: Only re-enable raycasts and reset visuals if the card ISN'T being played
        if (!isBeingPlayed)
        {
            if (_view.CanvasGroup != null)
                _view.CanvasGroup.blocksRaycasts = true;

            _view.ResetVisuals();
        }
        else
        {
            // Drag is over and the card is in flight to its play animation. Force
            // overrideSorting=true (it's been off since pool reset) and lower the
            // sortingOrder to CARD_IN_FLIGHT (800) so the card sits below the
            // selection canvas at 1000 while a queued RequestSelection action is
            // paused on player input. The play animation itself will overwrite
            // this with CARD_ANIMATION_GHOST_START (1500) when it captures the
            // card, so this won't affect the fly-to-discard visuals.
            _view.SetSortingState(true, CONSTANTS.CARD_IN_FLIGHT_SORTING_ORDER);
        }
    }

    public void ResetForPool()
    {
        // Calling it with default false is fine here, because we DO want visuals reset for pooling
        ResetInteractionState();

        _isDisplayOnly = false;

        // Remove card select logic
        OnCardSelectedToAdd = null;

        if (_view.CanvasGroup != null)
        {
            _view.CanvasGroup.interactable = true;
            _view.CanvasGroup.alpha = 1f; 
        }

        LayoutElement layoutElement = GetComponent<LayoutElement>();
        if (layoutElement != null) layoutElement.ignoreLayout = false;

        Canvas canvas = GetComponent<Canvas>(); 
        if (canvas != null)
        {
            // FIX: Turn off the override so the PlayerHand layout can manage sorting naturally
            canvas.overrideSorting = false; 
            canvas.sortingOrder = 0; 
        }

        isReleasedToPlay = false;
    }
}
