using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


[RequireComponent(typeof(RectTransform))]
public class NodeView : MonoBehaviour
{
    private Image _nodeImage;
    
    public MapNodeData Data { get; private set; }
    private RectTransform _rectTransform;
    
    [Header("Animation")]
    private Vector3 _origScale;
    [SerializeField] private float hoverScale;
    [SerializeField] private float hoverAnimationTime;
    [SerializeField] private float breathScale;
    [SerializeField] private float breathAnimationTime;
    
    private Vector3 _hoverTargetScale;
    private Vector3 _breathTargetScale;
    private Tween _breathingTween;
    private Tween _hoverTween;
    public Tween BreathingTween => _breathingTween;
    public Tween HoverTween => _hoverTween;
    private bool _isBreathing;

    private void Awake()
    {
        InitComponents();

        _origScale = transform.localScale;
        _breathTargetScale = _origScale * breathScale;
        _hoverTargetScale = _origScale * hoverScale;
    }

    private void Start()
    {
        
    }

    private void InitComponents()
    {
        if (_rectTransform == null) 
            _rectTransform = GetComponent<RectTransform>();
            
        if (_nodeImage == null) 
            _nodeImage = GetComponent<Image>();
    }

    private void OnEnable()
    {
        ToggleBreathing(Data.RuntimeNode.IsNext);
    }

    private void ToggleBreathing(bool isNextNode)
    {
        _isBreathing = isNextNode;
        if (_isBreathing)
        {
            if (_breathingTween != null && _breathingTween.IsActive()) return;
            _breathingTween = transform.DOScale(_breathTargetScale, breathAnimationTime).SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }
        else
        {
            _breathingTween?.Kill();
            _breathingTween = null;
        }
    }
    
    public void VisualHoverEnter()
    {
        _breathingTween?.Pause();
        _hoverTween?.Kill();
        _hoverTween = transform.DOScale(_hoverTargetScale, hoverAnimationTime).SetEase(Ease.OutBack);
    }

    public void VisualHoverExit()
    {
        _hoverTween?.Kill();

        _hoverTween = transform.DOScale(_origScale, hoverAnimationTime).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            if (!_isBreathing) return;
            _breathingTween?.Restart();
        });
    }
    public void Initialize(MapNodeData data)
    {
        Data = data;
        
        InitComponents();
        gameObject.name = $"NodeView_{data.Floor}_{data.Index}";
        
        // Apply position calculated by the generator
        _rectTransform.anchoredPosition = data.Position;

        if (data.RuntimeNode != null && data.RuntimeNode.NodeConfig != null)
        {
            _nodeImage.sprite = data.RuntimeNode.NodeConfig.NodeSprite;
        }
        else
        {
            _nodeImage.enabled =  false;
        }
    }

    public void RefreshNodeState()
    {
        if (Data == null || Data.RuntimeNode == null) return;
        if (Data.RuntimeNode.NodeConfig != null)
        {
            _nodeImage.sprite = Data.RuntimeNode.NodeConfig.NodeSprite;
        }
        
        ToggleBreathing(Data.RuntimeNode.IsNext);
    }
    
    private void OnDisable()
    {
        transform.DOKill();
        transform.localScale = _origScale;
    }
    private void OnDestroy()
    {
        transform.DOKill();
    }
}
