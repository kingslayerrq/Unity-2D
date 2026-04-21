using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public abstract class CardPileUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    protected CardPileManager _cardPileManager;
    
    protected TMP_Text _cardPileCardCount;
    protected Image _cardPileIconImage;
    protected Vector3 _cardPileIconOrigScale;
    protected Button _cardPileButton;
    protected Button _backButton;
    
    [SerializeField] protected ScrollRect _cardPileScrollRect;
    [SerializeField] private CardView cardViewPrefab;
    [SerializeField] private Transform cardPileViewContainer;
    [SerializeField] protected Transform _cardPileImageTransform;
    [SerializeField] protected CanvasGroup _cardPileCanvasGroup;
    
    [SerializeField] protected float _cardPileIconScale = 1.2f;
    [SerializeField] protected float _hoverAnimationTime = 0.2f;
    [SerializeField] protected float _cardPileCanvasFadeTime = 0.3f;
    
    // Kept static to share memory across all pile viewers
    protected static readonly Stack<CardView> _pooledViews = new Stack<CardView>();
    protected static readonly List<CardView> _activeViews = new List<CardView>();
    protected static readonly Stack<CardView> _rewardPooledViews = new Stack<CardView>();

    protected virtual void Awake()
    {
        _cardPileCardCount = GetComponentInChildren<TMP_Text>();
        _cardPileIconImage = GetComponentInChildren<Image>();
        _cardPileButton = GetComponentInChildren<Button>();

        if (_cardPileIconImage != null)
        {
            _cardPileIconOrigScale = _cardPileIconImage.transform.localScale;
        }

        if (_cardPileButton != null)
        {
            _cardPileButton.onClick.AddListener(DisplayCardsInPile);
        }

        if (_cardPileCanvasGroup != null)
        {
            _backButton = _cardPileCanvasGroup.GetComponentInChildren<Button>();
            if (_backButton != null)
            {
                _backButton.onClick.AddListener(HideCardsInPile);
            }
            
            // Ensure the canvas is hidden on awake
            _cardPileCanvasGroup.alpha = 0f;
            _cardPileCanvasGroup.interactable = false;
            _cardPileCanvasGroup.blocksRaycasts = false;
        }
    }

    // Call this from CardPileManager to pass the dependency down
    public virtual void Initialize(CardPileManager cardPileManager)
    {
        // Cleanup in case of re-initialization
        if (_cardPileManager != null)
        {
            _cardPileManager.OnCardPilesInitialized -= InitializeCardPileUI;
        }

        _cardPileManager = cardPileManager;

        if (_cardPileManager != null)
        {
            _cardPileManager.OnCardPilesInitialized += InitializeCardPileUI;
        }

        if (_cardPileButton != null)
        {
            _cardPileButton.onClick.RemoveAllListeners();
            _cardPileButton.onClick.AddListener(DisplayCardsInPile);
        }
    }
    
    public static void ReturnViewToRewardPool(CardView view)
    {
        if (view == null) return;
        view.ResetForPool();
        view.gameObject.SetActive(false);
        _rewardPooledViews.Push(view);
    }
    public static CardView GetOrCreateRewardView(CardView prefab, Transform parent)
    {
        CardView view;
        if (_rewardPooledViews.Count > 0)
        {
            view = _rewardPooledViews.Pop();
            view.transform.SetParent(parent, false);
        }
        else
        {
            view = Instantiate(prefab, parent);
        }

        view.gameObject.SetActive(true);
        return view;
    }


    protected abstract void InitializeCardPileUI();

    protected virtual void UpdateCardPileUI(int count)
    {
        if (_cardPileCardCount != null)
        {
            _cardPileCardCount.text = count.ToString();
        }
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
        
        _cardPileCanvasGroup.DOFade(0f, _cardPileCanvasFadeTime).OnComplete(() => 
        {
            // Pool the views when the fade out finishes so they don't sit active invisibly
            ClearAndPoolActiveViews();
        });
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

        CardInteraction interaction = view.GetComponent<CardInteraction>();
        if (interaction != null) interaction.SetDisplayOnly(true);

        return view;
    }
    
    protected void ReturnToPool(CardView cardView)
    {
        if (cardView == null) return;
        
        cardView.ResetForPool();
        cardView.gameObject.SetActive(false);
        _pooledViews.Push(cardView);
    }
    
    protected virtual void OnDestroy()
    {
        if (_cardPileManager != null)
        {
            _cardPileManager.OnCardPilesInitialized -= InitializeCardPileUI;
        }
        
        if (_cardPileButton != null)
        {
            _cardPileButton.onClick.RemoveListener(DisplayCardsInPile);
        }

        if (_backButton != null)
        {
            _backButton.onClick.RemoveListener(HideCardsInPile);
        }

        transform.DOKill();
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