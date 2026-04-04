using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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
    
    [Header("Drag & Drop")]
    private int origSiblingIndex;
    private Transform origParent;
    private GameObject placeholder;

    private void Awake()
    {
        _view = GetComponent<CardView>();
    }

    private void Update()
    {
        if (isSelected)
        {
            transform.position = Input.mousePosition;
            if (frameDelay)
            {
                frameDelay = false;
                return;
            }
            UpdateReceivers(Input.mousePosition);
        }
        
        if ((isSelected || isDragging) && Input.GetMouseButtonDown(1))
        {
            isSelected = false;
            isDragging = false;
            DropCard();
        }
    }

    private bool IsInteractionAllowed()
    {
        if (_view.CardInstance == null || _view.CardInstance.CurrentOwner == null) return false;
        return _view.CardInstance.CurrentOwner.CombatManager.CurrentCombatState ==
               CombatManager.CombatState.PlayerAction;
        
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsInteractionAllowed()) return;
        if (isInteractingWithAnyCard || isReleasedToPlay) return;
        _view.VisualHoverEnter();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isInteractingWithAnyCard || isReleasedToPlay) return;
        _view.VisualHoverExit();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!IsInteractionAllowed()) return;
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
        if (!IsInteractionAllowed()) return;
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
        _view.SetSortingOrder(999);
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
        if (_currentReceiver.CanReceiveCard(_view.CardInstance.CurrentOwner, _view.CardInstance))
        {
            var targets = _currentReceiver.GetReceiverUnitInstance(_view.CardInstance.CurrentOwner, _view.CardInstance);
            ResetInteractionState();
            _view.CardInstance.CurrentOwner.CombatManager.PlayCard(_view.CardInstance, targets);
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

    public void ResetInteractionState()
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

        if (_view.CanvasGroup != null)
            _view.CanvasGroup.blocksRaycasts = true;

        _view.ResetVisuals();
    }

    public void ResetForPool()
    {
        ResetInteractionState();

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
            canvas.overrideSorting = true;
            canvas.sortingOrder = 1; 
        }

        isReleasedToPlay = false;
    }
}