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
    
    [Header("Interaction State")]
    [Tooltip("A global flag so NO other cards react while one is being moved")] 
    private static bool isInteractingWithAnyCard = false; 
    
    // Local flags to track HOW this specific card is being moved
    private bool isSelected = false; 
    private bool isDragging = false; 
    [Tooltip("Prevents instant-dropping on the same frame as a click")] private bool frameDelay = false; 
    
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

    public void Refresh()
    {
        if (CardInstance == null) return;

        cardName.text = CardInstance.CurrentName;
        cardDescription.text = CardInstance.CurrentDescription;
        cardCost.text = CardInstance.CurrentCost.ToString();
        cardSprite.sprite = CardInstance.CurrentSprite;
    }
    
    public void ResetForReuse()
    {
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

        if (cardCanvas != null)
            cardCanvas.sortingOrder = 1;

        transform.DOKill();
        transform.localScale = _origScale;

        HideAllPopups();
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

    private void DropCard()
    {
        isInteractingWithAnyCard = false;
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
        if (isInteractingWithAnyCard) return;
        
        cardCanvas.sortingOrder = 100;

        transform.DOKill();
        transform.DOScale(_origScale * hoverScaleAmount, animDuration).SetEase(Ease.OutBack);

        AdaptPopupPosition();
        ShowAllPopups();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if  (isInteractingWithAnyCard) return;
        
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
            DropCard();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            transform.position = eventData.position;
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
}
