using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public abstract class CardPileUI: MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    protected CardPileManager _cardPileManager;
    protected TMP_Text _cardPileCardCount;
    protected Image _cardPileIconImage;
    protected Vector3 _cardPileIconOrigScale;
    protected Button _cardPileButton;
    protected Button _backButton;
    protected ScrollRect _cardPileScrollRect;
    [SerializeField] private CardView cardViewPrefab;
    [SerializeField] private Transform cardPileViewContainer;
    [SerializeField] protected Transform _cardPileImageTransform;
    [SerializeField] protected CanvasGroup _cardPileCanvasGroup;
    [SerializeField] protected float _cardPileIconScale;
    [SerializeField] protected float _hoverAnimationTime;
    [SerializeField] protected float _cardPileCanvasFadeTime;
    
    // Pooling
    protected static readonly Stack<CardView> _pooledViews = new Stack<CardView>();
    protected static readonly List<CardView> _activeViews = new List<CardView>();

    protected void Awake()
    {
        _cardPileManager = GetComponentInParent<CardPileManager>();
        _cardPileCardCount = GetComponentInChildren<TMP_Text>();
        _cardPileIconImage = GetComponentInChildren<Image>();
        _cardPileButton = GetComponentInChildren<Button>();
        _cardPileScrollRect = GetComponentInChildren<ScrollRect>();
        

        if (_cardPileIconImage != null)
        {
            _cardPileIconOrigScale = _cardPileIconImage.transform.localScale;
        }

        if (_cardPileCanvasGroup != null)
        {
            _backButton = _cardPileCanvasGroup.GetComponentInChildren<Button>();
            Debug.Log("Canvas active");
            if (_backButton != null)
            {
                Debug.Log("button active");
                _backButton.onClick.AddListener(HideCardsInPile);
            }
        }
        
    }

    protected void Start()
    {
        if (_cardPileManager == null)
        {
            Debug.LogError("CardPileManager not found");
            return;
        }
        _cardPileManager.OnCardPilesInitialized += InitializeCardPileUI;
        _cardPileButton.onClick.AddListener(DisplayCardsInPile);
    }

    protected abstract void InitializeCardPileUI();

    protected virtual void UpdateCardPileUI(int count)
    {
        if (_cardPileCardCount == null)
        {
            Debug.LogError("CardPileCardCount not found");
            return;
        }
        _cardPileCardCount.text = count.ToString();
    }

    protected virtual void DisplayCardsInPile()
    {
        if (_cardPileCanvasGroup == null) return;
        
        _cardPileCanvasGroup.DOFade(1f, _cardPileCanvasFadeTime);
        
        _cardPileCanvasGroup.interactable = true;
        _cardPileCanvasGroup.blocksRaycasts = true;
    }

    public virtual void HideCardsInPile()
    {
        if (_cardPileCanvasGroup == null) return;

        _cardPileCanvasGroup.interactable = false;
        _cardPileCanvasGroup.blocksRaycasts = false;
        
        _cardPileCanvasGroup.DOFade(0f, _cardPileCanvasFadeTime);
    }

    protected void RenderCardViews(IReadOnlyList<CardInstance> cards)
    {
        ClearAndPoolActiveViews();

        if (cards != null)
        {
            foreach (var card in cards)
            {
                CardView view = GetOrCreateView();
                view.Bind(card);
                _activeViews.Add(view);
            }
        }
    }
    
    protected void ClearAndPoolActiveViews()
    {
        foreach (var view in _activeViews)
        {
            ReturnToPool(view);
        }
        _activeViews.Clear();
    }

    protected CardView GetOrCreateView()
    {
        CardView view;
        if (_pooledViews.Count > 0)
        {
            view = _pooledViews.Pop();
        }
        else
        {
            view = Instantiate(cardViewPrefab, cardPileViewContainer);
        }
        
        view.gameObject.SetActive(true);
        view.transform.SetParent(cardPileViewContainer, false);
        
        Canvas canvas = view.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.overrideSorting = false;
        }

        return view;
    }
    
    protected void ReturnToPool(CardView cardView)
    {
        if (cardView == null) return;
        
        cardView.ResetForPool();
        cardView.gameObject.SetActive(false);
        _pooledViews.Push(cardView);
    }
    
    protected virtual void OnDisable()
    {
        if (_cardPileManager == null)  return;
        _cardPileManager.OnCardPilesInitialized -= InitializeCardPileUI;
        _cardPileButton.onClick.RemoveAllListeners();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_cardPileImageTransform == null) return;
        _cardPileImageTransform.DOKill();
        _cardPileImageTransform.DOScale(_cardPileIconOrigScale * _cardPileIconScale, _hoverAnimationTime);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_cardPileImageTransform == null) return;
        _cardPileImageTransform.DOKill();
        _cardPileImageTransform.DOScale(_cardPileIconOrigScale, _hoverAnimationTime);
    }
}
