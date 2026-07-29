using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CanvasGroup))]
public sealed class FriendListSidebar : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private Key toggleKey = Key.P;

    [Header("Animation")]
    [SerializeField, Min(0f)] private float slideDuration = 0.25f;
    [SerializeField] private Ease slideEase = Ease.InOutCubic;
    [SerializeField] private bool startsVisible = true;

    private RectTransform panel;
    private CanvasGroup canvasGroup;
    private Tween slideTween;

    private Vector2 shownPosition;
    private bool isVisible;
    private bool initialized;

    private float HiddenX => shownPosition.x + panel.rect.width;

    private void Awake()
    {
        panel = (RectTransform)transform;
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        // Ensures panel.rect.width reflects the Canvas and CanvasScaler.
        Canvas.ForceUpdateCanvases();

        shownPosition = panel.anchoredPosition;
        isVisible = startsVisible;
        initialized = true;

        SetPositionImmediately(isVisible);
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard != null &&
            keyboard[toggleKey].wasPressedThisFrame)
        {
            SetVisible(!isVisible);
        }
    }

    public void SetVisible(bool visible)
    {
        if (!initialized)
        {
            return;
        }

        isVisible = visible;

        // Allows repeated key presses while it is still moving.
        slideTween?.Kill();

        SetInteractionEnabled(visible);

        slideTween = panel
            .DOAnchorPosX(visible ? shownPosition.x : HiddenX, slideDuration)
            .SetEase(slideEase)
            .SetUpdate(true);
    }

    private void SetPositionImmediately(bool visible)
    {
        Vector2 position = shownPosition;
        position.x = visible ? shownPosition.x : HiddenX;
        panel.anchoredPosition = position;

        SetInteractionEnabled(visible);
    }

    private void SetInteractionEnabled(bool enabled)
    {
        canvasGroup.interactable = enabled;
        canvasGroup.blocksRaycasts = enabled;
    }

    private void OnRectTransformDimensionsChange()
    {
        if (!initialized || isVisible)
        {
            return;
        }

        // Keep it fully hidden after a resolution/window-size change.
        slideTween?.Kill();
        SetPositionImmediately(false);
    }

    private void OnDestroy()
    {
        slideTween?.Kill();
    }
}