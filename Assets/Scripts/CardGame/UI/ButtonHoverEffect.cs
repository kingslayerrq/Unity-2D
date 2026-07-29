using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using UnityEngine.Serialization;

[RequireComponent(typeof(Button))]
public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [Header("UI References")]
    [SerializeField] private Image highlightBorder;
    [SerializeField] private TMP_Text text;
    
    [Header("Settings")]
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private float liftAmount = 2f;
    [SerializeField] private float dimAmount = 6f;
    [SerializeField] private float liftDuration = 0.1f;
    [SerializeField] private float dimDuration = 0.2f;
    [SerializeField] private float recoverDuration = 0.2f;
    [SerializeField] private Color textHighlightColor = Color.red;

    private Button _button;
    private Color _origTextColor;
    private RectTransform _rectTransform;
    private float _originalY;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _rectTransform = GetComponent<RectTransform>();
        
        // Save the original Y position so we know where to return to
        _originalY = _rectTransform.anchoredPosition.y;

        // Ensure the border starts invisible
        if (highlightBorder != null)
        {
            Color startColor = highlightBorder.color;
            startColor.a = 0f;
            highlightBorder.color = startColor;
        }
        
        // save orig text color
        if (text != null)
        {
            _origTextColor = text.color;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Only show the highlight if the button is actually interactable
        if (_button.interactable && highlightBorder != null)
        {
            highlightBorder.DOKill();
            highlightBorder.DOFade(1f, fadeDuration);
            text.color = textHighlightColor;
            _rectTransform.DOKill();
            _rectTransform.DOAnchorPosY(_originalY + liftAmount, liftDuration).SetEase(Ease.OutQuad);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (highlightBorder != null)
        {
            highlightBorder.DOKill();
            highlightBorder.DOFade(0f, fadeDuration);
            text.color = _origTextColor;
        }
        
        if (_button.interactable)
        {
            _rectTransform.DOKill();
            _rectTransform.DOAnchorPosY(_originalY, recoverDuration).SetEase(Ease.OutQuad);
        }
    }

    private void OnDisable()
    {
        // If the button gets disabled, immediately hide the border
        if (highlightBorder != null)
        {
            highlightBorder.DOKill();
            Color c = highlightBorder.color;
            c.a = 0f;
            highlightBorder.color = c;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (highlightBorder != null)
        {
            highlightBorder.DOKill();
            highlightBorder.DOFade(0f, fadeDuration);
            text.color = _origTextColor;
        }
        if (_button.interactable)
        {
            _rectTransform.DOKill();
            _rectTransform.DOAnchorPosY(_originalY - dimAmount, dimDuration).SetEase(Ease.OutQuad);
        }
    }
}