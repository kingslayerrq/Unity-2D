using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class CardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler,
    IEndDragHandler, IDragHandler, IPointerClickHandler
{
    
    [SerializeField] private Canvas cardCanvas;
    [SerializeField] private Image cardSprite;
    [SerializeField] private TMP_Text cardName;
    [SerializeField] private TMP_Text cardDescription;
    [SerializeField] private TMP_Text cardCost;
    
    [SerializeField] private Transform cardKeywordPopupParent;
    [SerializeField] private KeywordPopupView cardKeywordPopupPrefab;
    private readonly List<KeywordPopupView> _cardKeywordPopups =  new List<KeywordPopupView>();

    
    private CardAnimation _cardAnimation;
    public CardAnimation CardAnimation => _cardAnimation;
    
    [Header("Interaction State")]
    [Tooltip("A global flag so NO other cards react while one is being moved")] 
    private static bool isInteractingWithAnyCard = false;

    private ICardDropReceiver _currentReceiver = null;
    
    // Local flags to track HOW this specific card is being moved
    private bool isReleasedToPlay = false;
    private bool isSelected = false; 
    private bool isDragging = false; 
    [Tooltip("Prevents instant-dropping on the same frame as a click")] 
    private bool frameDelay = false; 
    
    [Header("Drag & Drop")]
    [SerializeField] private int origSiblingIndex;
    private Transform origParent;
    private GameObject placeholder;
    private CanvasGroup _canvasGroup;
    
    [Header("Tooltip Positioning")]
    [SerializeField] private RectTransform popupParentRect; 
    [SerializeField] private float popupSpacing = 4f;
    [SerializeField] private RectTransform playerHandRect; 
    
    [Header("Layout element")]
    [SerializeField] private float cellWidth = 230f; 
    [SerializeField] private float gridSpacingX = 2f;
    
    [Header("DOTween")] 
    [SerializeField] private float hoverScaleAmount;
    [SerializeField] private float animDuration;
    private Vector3 _origScale;

    public CardInstance CardInstance { get; private set; }

    private void Awake()
    {
        _origScale = transform.localScale;
        _canvasGroup = GetComponent<CanvasGroup>();
        _cardAnimation = GetComponent<CardAnimation>();
    }

    private void Update()
    {
        if (isSelected)
        {
            transform.position = Input.mousePosition;
            // Wait one frame so the release of the initial click doesn't trigger the drop
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

    public void Bind(CardInstance instance)
    {
        if (CardInstance == instance)
        {
            Refresh();
            return;
        }

        CardInstance = instance;

        foreach (Transform child in cardKeywordPopupParent)
        {
            Destroy(child.gameObject);
        }
        _cardKeywordPopups.Clear();

        foreach (var keyword in CardInstance.Data.Keywords)
        {
            var kwp = Instantiate(cardKeywordPopupPrefab, cardKeywordPopupParent);
            kwp.Setup(keyword);
            _cardKeywordPopups.Add(kwp);
            kwp.gameObject.SetActive(false);
        }

        Refresh();
    }

    // <summary>
    // Refresh the view based on CardInstance data
    // <summary>
    public void Refresh()
    {
        if (CardInstance == null) return;

        cardName.text = CardInstance.CurrentName;
        cardDescription.text = CardInstance.CurrentDescription;
        cardCost.text = CardInstance.CurrentCost.ToString();
        cardSprite.sprite = CardInstance.CurrentSprite;
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

        if (_canvasGroup != null)
            _canvasGroup.blocksRaycasts = true;

        transform.DOKill();
        transform.localScale = _origScale;

        HideAllPopups();
    }
    public void ResetForPool()
    {
        // 1. Run your standard interaction reset (kills tweens, resets scale)
        ResetInteractionState();

        // 2. Restore CanvasGroup interactability
        // (blocksRaycasts is already reset in ResetInteractionState, but we need interactable too)
        if (_canvasGroup != null)
        {
            _canvasGroup.interactable = true;
            _canvasGroup.alpha = 1f; // Just in case you add fade animations later
        }

        // 3. Re-enable Layout Group control
        // The animation set ignoreLayout = true so it could fly freely. We must turn it off.
        LayoutElement layoutElement = GetComponent<LayoutElement>();
        if (layoutElement != null)
        {
            layoutElement.ignoreLayout = false;
        }

        // 4. Reset Canvas sorting
        // The animation forced overrideSorting = true to bring it to the front.
        Canvas canvas = GetComponent<Canvas>(); 
        if (canvas != null)
        {
            canvas.overrideSorting = false;
            canvas.sortingOrder = 1; // Or whatever your default base sorting order is
        }

        // 5. Reset physical rotation and position
        // DOTween spins the card and moves it. The layout group handles position, 
        // but the rotation needs to be explicitly zeroed out.
        transform.localRotation = Quaternion.identity;
        transform.localPosition = Vector3.zero;

        isReleasedToPlay = false;
    }

    private void PickUpCard()
    {
        isInteractingWithAnyCard = true;
        HideAllPopups();
        
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
        cardCanvas.sortingOrder = 999;
        
        _canvasGroup.blocksRaycasts = false;
    }

    // <summary>
    // If the current receiver can receive card
    // Then enqueue SpendEnergyAction & PlayCardAction to queue
    // <summary>
    private bool TryPlayCard()
    {
        if (_currentReceiver == null) return false;
        if (_currentReceiver.CanReceiveCard(CardInstance.CurrentOwner, CardInstance))
        {
            // Cache
            var targets = _currentReceiver.GetReceiverUnitInstance(CardInstance.CurrentOwner, CardInstance);
            // Clear 
            ResetInteractionState();
            
            CardInstance.CurrentOwner.CombatManager.PlayCard(CardInstance, targets);
            return true;
        }
        return false;
    }
    
    // <summary>
    // Release the card to play, If receiver is null, it will fail
    // <summary>
    private void ReleaseCard(Vector2 screenPos)
    {
        
        UpdateReceivers(screenPos);
        
        isReleasedToPlay = true;
        
        // Play the Card (removes the card from hand data)
        bool isPlayed = TryPlayCard();
        
        ClearHoveredReceiver();

        if (!isPlayed) DropCard();

    }
    
    
    private void DropCard()
    {
        ClearHoveredReceiver();
        
        isInteractingWithAnyCard = false;
        isReleasedToPlay = false;
        isSelected = false;
        isDragging = false;
        frameDelay = false;
        _canvasGroup.blocksRaycasts = true;

        Destroy(placeholder);

        transform.SetParent(origParent);
        transform.SetSiblingIndex(origSiblingIndex);

        cardCanvas.sortingOrder = 1;
        transform.DOKill();
        transform.DOScale(_origScale, animDuration).SetEase(Ease.OutQuad);
    }

    private void HideAllPopups()
    {
        foreach (var kwp in _cardKeywordPopups)
        {
            kwp.Hide();
        }
    }

    private void ShowAllPopups()
    {
        foreach (var kwp in _cardKeywordPopups)
        {
            kwp.Show();
        }
    }

    private void AdaptPopupPosition()
    {
        // 1. Calculate how many columns the popups will take up
        int popupCount = _cardKeywordPopups.Count;
        
        int columns = Mathf.CeilToInt((float)popupCount / 3f); 
        
        // 2. Calculate the local UI width of those columns
        float localTooltipWidth = (columns * cellWidth) + (Mathf.Max(0, columns - 1) * gridSpacingX);

        // 3. Convert that local width into true World Space width
        // (Crucial if your Canvas scales with screen size or your card is scaled!)
        float worldTooltipWidth = localTooltipWidth * popupParentRect.lossyScale.x;
        float worldSpacing = popupSpacing * popupParentRect.lossyScale.x;

        // 4. Get the exact right edge of the Player Hand panel in World Space
        Vector3[] handCorners = new Vector3[4];
        playerHandRect.GetWorldCorners(handCorners);
        float handRightEdgeX = handCorners[2].x; // corners[2] is Top Right

        // 5. Get the exact right edge of the hovered Card in World Space
        Vector3[] cardCorners = new Vector3[4];
        GetComponent<RectTransform>().GetWorldCorners(cardCorners);
        float cardRightEdgeX = cardCorners[2].x;
        
        bool fitsInPanel = (cardRightEdgeX + worldSpacing + worldTooltipWidth) < handRightEdgeX;
        
        if (fitsInPanel)
        {
            // Move container to the LEFT side of the card
            popupParentRect.pivot = new Vector2(1f, 0.5f); // Pivot on the container's right edge
            popupParentRect.anchorMin = new Vector2(0f, 0.5f); // Anchor to the card's left edge
            popupParentRect.anchorMax = new Vector2(0f, 0.5f);
            popupParentRect.anchoredPosition = new Vector2(-popupSpacing, 0f); // Shift left by the spacing amount
        }
        else
        {
            // Move container to the RIGHT side of the card
            popupParentRect.pivot = new Vector2(0f, 0.5f); // Pivot on the container's left edge
            popupParentRect.anchorMin = new Vector2(1f, 0.5f); // Anchor to the card's right edge
            popupParentRect.anchorMax = new Vector2(1f, 0.5f);
            popupParentRect.anchoredPosition = new Vector2(popupSpacing, 0f); // Shift right by the spacing amount
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isInteractingWithAnyCard || isReleasedToPlay) return;
        
        cardCanvas.sortingOrder = 100;

        transform.DOKill();
        transform.DOScale(_origScale * hoverScaleAmount, animDuration).SetEase(Ease.OutBack);

        AdaptPopupPosition();
        ShowAllPopups();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if  (isInteractingWithAnyCard || isReleasedToPlay) return;
        
        cardCanvas.sortingOrder = 1;
        
        transform.DOKill();
        transform.DOScale(_origScale, animDuration).SetEase(Ease.OutQuad);

        
        HideAllPopups();
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        
        // If it's already attached to the mouse via click, don't start a drag
        if (isSelected) return;

        if (!isInteractingWithAnyCard)
        {
            isDragging = true;
            PickUpCard();
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

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            transform.position = eventData.position;
            UpdateReceivers(eventData.position);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        // If we just finished a drag, don't accidentally register it as a click
        if (isDragging) return;

        if (!isInteractingWithAnyCard)
        {
            isSelected = true;
            frameDelay = true;
            PickUpCard();
        }
    }

    private ICardDropReceiver FindReceiver(Vector2 screenPos)
    {
        // 1. Set up the Pointer Event Data at the mouse position
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = screenPos;

        // 2. Create a list to store the results
        List<RaycastResult> results = new List<RaycastResult>();

        // 3. Raycast using the UI System's raycaster
        EventSystem.current.RaycastAll(eventData, results);
        ICardDropReceiver bestReceiver = null;
        int bestPriority = int.MinValue;
        bool hitPlayCardArea = false;
        ICardDropReceiver playCardArea = null;
        foreach (RaycastResult result in results)
        {
            // Search the hit object for the interface
            ICardDropReceiver receiver = result.gameObject.GetComponentInParent<ICardDropReceiver>();
            if (!hitPlayCardArea && receiver is PlayCardArea)
            {
                hitPlayCardArea = true;
                playCardArea = receiver;
            }
            if (receiver == null) continue;

            if (!receiver.CanReceiveCard(CardInstance.CurrentOwner, CardInstance))
                continue;

            if (bestReceiver == null || receiver.Priority > bestPriority)
            {
                bestReceiver = receiver;
                bestPriority = receiver.Priority;
            }
        }

        if (bestReceiver == null && playCardArea != null) return playCardArea;
        return bestReceiver;
    }

    private void UpdateReceivers(Vector2 screenPos)
    {
        ICardDropReceiver newReceiver = FindReceiver(screenPos);

        if (ReferenceEquals(newReceiver, _currentReceiver))
        {
            return;
        }

        if (_currentReceiver != null)
        {
            _currentReceiver.OnHoverExit(CardInstance.CurrentOwner, CardInstance);
        }

        _currentReceiver = newReceiver;
        

        if (_currentReceiver != null)
        {
            // Snaps the card back to hand if we can't play this
            if (!CardInstance.CanBePlayed )
            {
                DropCard();
                _currentReceiver =  null;
                return;
            }
            _currentReceiver.OnHoverEnter(CardInstance.CurrentOwner, CardInstance);
        }
        
    }
    
    private void ClearHoveredReceiver()
    {
        if (_currentReceiver != null)
        {
            _currentReceiver.OnHoverExit(CardInstance.CurrentOwner, CardInstance);
            _currentReceiver = null;
        }
    }
}
