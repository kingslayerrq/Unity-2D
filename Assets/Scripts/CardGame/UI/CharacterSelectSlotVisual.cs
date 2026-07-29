using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class CharacterSlotVisual :
    MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("Slot Data")]
    [SerializeField] private Button button;
    private bool isUnlocked;

    private PlayerUnitData character;
    private int slotIndex;
    private Action<int> clickedCallback;
    
    [Header("References")]
    [Tooltip("The child object that scales. Do not assign the layout root.")]
    [SerializeField] private RectTransform visualRoot;

    [Tooltip("The character portrait that changes between greyed and full color.")]
    [SerializeField] private Image portraitImage;

    [Tooltip("CanvasGroup placed on the selection outline object.")]
    [SerializeField] private CanvasGroup selectionOutlineCanvasGroup;

    [SerializeField] private Sprite characterPortraitSprite;

    [Header("Scale")]
    [SerializeField, Min(0f)]
    private float normalScale = 1f;

    [SerializeField, Min(0f)]
    private float hoverScale = 1.04f;

    [SerializeField, Min(0f)]
    private float selectedScale = 1.06f;

    [SerializeField, Min(0f)]
    private float scaleTransitionSpeed = 12f;

    [Header("Portrait Color")]
    [SerializeField]
    private Color normalPortraitColor =
        new Color(0.55f, 0.55f, 0.55f, 1f);

    [SerializeField]
    private Color hoverPortraitColor = Color.white;

    [SerializeField]
    private Color selectedPortraitColor = Color.white;

    [SerializeField, Min(0f)]
    private float colorTransitionSpeed = 12f;

    [Header("Selected Outline Pulse")]
    [SerializeField, Range(0f, 1f)]
    private float minimumOutlineAlpha = 0.3f;

    [SerializeField, Range(0f, 1f)]
    private float maximumOutlineAlpha = 1f;

    [Tooltip("Number of complete pulse cycles per second.")]
    [SerializeField, Min(0.01f)]
    private float pulseCyclesPerSecond = 1.25f;

    [Tooltip("How quickly the outline fades out after deselection.")]
    [SerializeField, Min(0f)]
    private float outlineFadeSpeed = 8f;

    private bool isHovered;
    private bool isSelected;
    private float pulseElapsedTime;
    
    public PlayerUnitData Character => character;
    public int SlotIndex => slotIndex;
    public bool IsUnlocked => isUnlocked;

    private void Awake()
    {
        ValidateReferences();
        ApplyInitialState();
    }

    private void OnDisable()
    {
        isHovered = false;
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(HandleButtonClicked);

        clickedCallback = null;
    }

    private void Update()
    {
        float deltaTime = Time.unscaledDeltaTime;

        UpdateScale(deltaTime);
        UpdatePortraitColor(deltaTime);
        UpdateOutline(deltaTime);
    }
    
    public void Initialize(
        PlayerUnitData characterData,
        bool unlocked,
        int index,
        Action<int> onClicked)
    {
        if (characterData == null)
        {
            Debug.LogError(
                "Cannot initialize a character slot with null data.",
                this);

            return;
        }

        if (button == null)
        {
            Debug.LogError(
                "CharacterSlotVisual requires a Button reference.",
                this);

            return;
        }

        character = characterData;
        isUnlocked = unlocked;
        slotIndex = index;
        clickedCallback = onClicked;

        button.onClick.RemoveListener(HandleButtonClicked);
        button.onClick.AddListener(HandleButtonClicked);

        button.interactable = isUnlocked;

        isHovered = false;
        SetSelected(false);

        ApplyCharacterVisuals();
        ApplyInitialState();
    }

    private void HandleButtonClicked()
    {
        if (!isUnlocked)
            return;

        clickedCallback?.Invoke(slotIndex);
    }

    private void ApplyCharacterVisuals()
    {
        portraitImage.sprite = isUnlocked
            ? character.PlayerCharacterSelectPfpUnlocked
            : character.PlayerCharacterSelectPfpLocked;
    }

    /// <summary>
    /// Called by the character selection menu when this slot becomes
    /// selected or deselected.
    /// </summary>
    public void SetSelected(bool selected)
    {
        selected = selected && isUnlocked;

        if (isSelected == selected)
            return;

        isSelected = selected;

        if (isSelected)
            pulseElapsedTime = 0f;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isUnlocked)
            return;

        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }

    private void UpdateScale(float deltaTime)
    {
        float targetScale;

        if (!isUnlocked)
        {
            targetScale = normalScale;
        }
        else if (isSelected)
        {
            targetScale = selectedScale;
        }
        else if (isHovered)
        {
            targetScale = hoverScale;
        }
        else
        {
            targetScale = normalScale;
        }

        Vector3 target = Vector3.one * targetScale;
        float interpolation = GetInterpolation(
            scaleTransitionSpeed,
            deltaTime);

        visualRoot.localScale = Vector3.Lerp(
            visualRoot.localScale,
            target,
            interpolation);
    }

    private void UpdatePortraitColor(float deltaTime)
    {
        if (!isUnlocked)
        {
            portraitImage.color = Color.white;
            return;
        }

        Color targetColor;

        if (isSelected)
        {
            targetColor = selectedPortraitColor;
        }
        else if (isHovered)
        {
            targetColor = hoverPortraitColor;
        }
        else
        {
            targetColor = normalPortraitColor;
        }

        float interpolation = GetInterpolation(
            colorTransitionSpeed,
            deltaTime);

        portraitImage.color = Color.Lerp(
            portraitImage.color,
            targetColor,
            interpolation);
    }

    private void UpdateOutline(float deltaTime)
    {
        if (!isUnlocked)
        {
            selectionOutlineCanvasGroup.alpha = 0f;
            return;
        }
        
        if (isSelected)
        {
            pulseElapsedTime += deltaTime;

            float radians =
                pulseElapsedTime *
                pulseCyclesPerSecond *
                Mathf.PI *
                2f;

            // Produces a 0–1 value that begins at zero.
            float pulse =
                (Mathf.Sin(radians - Mathf.PI * 0.5f) + 1f) * 0.5f;

            selectionOutlineCanvasGroup.alpha = Mathf.Lerp(
                minimumOutlineAlpha,
                maximumOutlineAlpha,
                pulse);

            return;
        }

        // Hovering does not display or pulse the outline.
        selectionOutlineCanvasGroup.alpha = Mathf.MoveTowards(
            selectionOutlineCanvasGroup.alpha,
            0f,
            outlineFadeSpeed * deltaTime);
    }

    private void ApplyInitialState()
    {
        visualRoot.localScale = Vector3.one * normalScale;
        portraitImage.color = normalPortraitColor;
        selectionOutlineCanvasGroup.alpha = 0f;
    }

    private void ValidateReferences()
    {
        if (visualRoot == null)
        {
            Debug.LogError(
                $"{nameof(CharacterSlotVisual)} on {name} is missing " +
                $"{nameof(visualRoot)}.",
                this);
        }

        if (portraitImage == null)
        {
            Debug.LogError(
                $"{nameof(CharacterSlotVisual)} on {name} is missing " +
                $"{nameof(portraitImage)}.",
                this);
        }

        if (selectionOutlineCanvasGroup == null)
        {
            Debug.LogError(
                $"{nameof(CharacterSlotVisual)} on {name} is missing " +
                $"{nameof(selectionOutlineCanvasGroup)}.",
                this);
        }
    }

    private static float GetInterpolation(float speed, float deltaTime)
    {
        if (speed <= 0f)
            return 1f;

        return 1f - Mathf.Exp(-speed * deltaTime);
    }
}