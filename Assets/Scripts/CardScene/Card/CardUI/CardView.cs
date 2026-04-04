using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class CardView : MonoBehaviour
{
    [SerializeField] private Canvas cardCanvas;
    [SerializeField] private Image cardSprite;
    [SerializeField] private TMP_Text cardName;
    [SerializeField] private TMP_Text cardDescription;
    [SerializeField] private TMP_Text cardCost;
    
    [SerializeField] private Transform cardKeywordPopupParent;
    [SerializeField] private KeywordPopupView cardKeywordPopupPrefab;
    private readonly List<KeywordPopupView> _cardKeywordPopups = new List<KeywordPopupView>();

    [Header("Tooltip Positioning")]
    public RectTransform popupParentRect; 
    public float popupSpacing = 4f;
    public RectTransform playerHandRect; 
    
    [Header("Layout element")]
    public float cellWidth = 230f; 
    public float gridSpacingX = 2f;
    
    [Header("DOTween")] 
    [SerializeField] private float hoverScaleAmount = 1.2f;
    [SerializeField] private float animDuration = 0.2f;
    
    private Vector3 _origScale;
    public CanvasGroup CanvasGroup { get; private set; }
    public CardAnimation CardAnimation { get; private set; }
    public CardInstance CardInstance { get; private set; }
    private CardInteraction _cardInteraction;

    private void Awake()
    {
        _origScale = transform.localScale;
        CanvasGroup = GetComponent<CanvasGroup>();
        CardAnimation = GetComponent<CardAnimation>();
        TryGetComponent(out _cardInteraction);
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
    
    public void ResetForPool()
    {
        if (_cardInteraction != null)
        {
            _cardInteraction.ResetForPool();
        }
        else
        {
            // Fallback for display-only cards
            ResetVisuals();
            
            if (CanvasGroup != null)
            {
                CanvasGroup.interactable = true;
                CanvasGroup.alpha = 1f; 
            }
        }
    }

    // --- Visual Triggers called by CardInteraction ---

    public void VisualHoverEnter()
    {
        SetSortingOrder(100);
        transform.DOKill();
        transform.DOScale(_origScale * hoverScaleAmount, animDuration).SetEase(Ease.OutBack);
        AdaptPopupPosition();
        ShowAllPopups();
    }

    public void VisualHoverExit()
    {
        SetSortingOrder(1);
        transform.DOKill();
        transform.DOScale(_origScale, animDuration).SetEase(Ease.OutQuad);
        HideAllPopups();
    }

    public void SetSortingOrder(int order)
    {
        cardCanvas.sortingOrder = order;
    }

    public void ResetVisuals()
    {
        transform.DOKill();
        transform.localScale = _origScale;
        transform.localRotation = Quaternion.identity;
        transform.localPosition = Vector3.zero;
        SetSortingOrder(1); // Reset to base sorting
        HideAllPopups();
    }

    public void HideAllPopups()
    {
        foreach (var kwp in _cardKeywordPopups) kwp.Hide();
    }

    private void ShowAllPopups()
    {
        foreach (var kwp in _cardKeywordPopups) kwp.Show();
    }

    private void AdaptPopupPosition()
    {
        // 1. Calculate how many columns the popups will take up
        int popupCount = _cardKeywordPopups.Count;
        int columns = Mathf.CeilToInt((float)popupCount / 3f); 
        
        float localTooltipWidth = (columns * cellWidth) + (Mathf.Max(0, columns - 1) * gridSpacingX);
        float worldTooltipWidth = localTooltipWidth * popupParentRect.lossyScale.x;
        float worldSpacing = popupSpacing * popupParentRect.lossyScale.x;

        Vector3[] handCorners = new Vector3[4];
        playerHandRect.GetWorldCorners(handCorners);
        float handRightEdgeX = handCorners[2].x;

        Vector3[] cardCorners = new Vector3[4];
        GetComponent<RectTransform>().GetWorldCorners(cardCorners);
        float cardRightEdgeX = cardCorners[2].x;
        
        bool fitsInPanel = (cardRightEdgeX + worldSpacing + worldTooltipWidth) < handRightEdgeX;
        
        if (fitsInPanel)
        {
            popupParentRect.pivot = new Vector2(1f, 0.5f); 
            popupParentRect.anchorMin = new Vector2(0f, 0.5f); 
            popupParentRect.anchorMax = new Vector2(0f, 0.5f);
            popupParentRect.anchoredPosition = new Vector2(-popupSpacing, 0f); 
        }
        else
        {
            popupParentRect.pivot = new Vector2(0f, 0.5f); 
            popupParentRect.anchorMin = new Vector2(1f, 0.5f); 
            popupParentRect.anchorMax = new Vector2(1f, 0.5f);
            popupParentRect.anchoredPosition = new Vector2(popupSpacing, 0f); 
        }
    }
}