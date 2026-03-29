using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class CardAnimation : MonoBehaviour
{
    [Header("Play Animation")]
    [SerializeField] private float flyToCenterDuration = 0.2f;
    [SerializeField] private float centerPauseDuration = 0.1f;
    [SerializeField] private float flyToDiscardDuration = 0.3f;
    [SerializeField] private float centerScale = 1.1f;
    [SerializeField] private float discardScale = 0.15f;
    [SerializeField] private float discardRotateAmount = 360f;
    
    [SerializeField] private float burstWindow = 0.25f;
    [SerializeField] private float burstIndentX = 24f;
    [SerializeField] private float burstIndentY = -8f;
    [SerializeField] private int maxBurstIndentSteps = 4;
    
    private const int GhostSortingStart = 10000;
    private static int nextGhostSortingOrder = GhostSortingStart;
    
    private static float lastGhostSpawnTime = -999f;
    private static int currentBurstIndex = 0;

    private static int GetNextGhostSortingOrder()
    {
        // earlier spawned card stays on top
        if (nextGhostSortingOrder < 9000)
        {
            nextGhostSortingOrder = GhostSortingStart;
        }

        return nextGhostSortingOrder--;
    }
    

    private int GetBurstIndex()
    {
        float now = Time.unscaledTime;

        if (now - lastGhostSpawnTime <= burstWindow)
        {
            currentBurstIndex++;
        }
        else
        {
            currentBurstIndex = 0;
        }

        lastGhostSpawnTime = now;
        return Mathf.Min(currentBurstIndex, maxBurstIndentSteps);
    }

    public Sequence PlayToDiscardAnimationSelf(Canvas rootCanvas, RectTransform discardIcon)
    {
        if (rootCanvas == null || discardIcon == null)
        {
            Debug.LogWarning("Missing rootCanvas or discardIcon for play animation.");
            return null;
        }

        RectTransform cardRect = GetComponent<RectTransform>();
        if (cardRect == null)
        {
            Debug.LogWarning("Card is missing RectTransform.");
            return null;
        }

        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Prevent interaction during animation
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        // Optional: if this card is inside a LayoutGroup, this helps stop layout control
        LayoutElement layoutElement = GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = gameObject.AddComponent<LayoutElement>();
        }
        layoutElement.ignoreLayout = true;

        // Bring this actual card to the top-level canvas so it can animate freely
        transform.SetParent(rootCanvas.transform, true);

        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
        }

        canvas.overrideSorting = true;
        canvas.sortingOrder = GetNextGhostSortingOrder();

        Vector3 centerWorldPos = GetCanvasWorldPoint(
            rootCanvas,
            new Vector2(Screen.width * 0.55f, Screen.height * 0.6f)
        );

        int burstIndex = GetBurstIndex();
        Vector3 burstOffset = new Vector3(
            burstIndex * burstIndentX,
            burstIndex * burstIndentY,
            0f
        );
        Vector3 centerTarget = centerWorldPos + burstOffset;

        Sequence seq = DOTween.Sequence();

        seq.Append(cardRect.DOMove(centerTarget, flyToCenterDuration).SetEase(Ease.OutCubic));
        seq.Join(cardRect.DOScale(centerScale, flyToCenterDuration).SetEase(Ease.OutBack));

        seq.AppendInterval(centerPauseDuration);

        seq.Append(cardRect.DOMove(discardIcon.position, flyToDiscardDuration).SetEase(Ease.InBack));
        seq.Join(cardRect.DOScale(discardScale, flyToDiscardDuration).SetEase(Ease.InBack));
        seq.Join(
            cardRect.DORotate(
                new Vector3(0f, 0f, discardRotateAmount),
                flyToDiscardDuration,
                RotateMode.FastBeyond360
            )
        );
        
        return seq;
    }
    /// <summary>
    /// Spawns a visual ghost of this card and animates it toward the discard pile.
    /// Returns the tween sequence so callers can wait for completion.
    /// </summary>
    public Sequence PlayToDiscardAnimation(Canvas rootCanvas, RectTransform discardIcon)
    {
        if (rootCanvas == null || discardIcon == null)
        {
            Debug.LogWarning("Missing rootCanvas or discardIcon for play animation.");
            return null;
        }

        RectTransform originalRect = GetComponent<RectTransform>();

        GameObject ghost = Instantiate(gameObject, rootCanvas.transform);
        ghost.name = $"{gameObject.name}_Ghost";

        CardView ghostCardView = ghost.GetComponent<CardView>();
        if (ghostCardView != null)
        {
            Destroy(ghostCardView);
        }

        CardAnimation ghostAnimation = ghost.GetComponent<CardAnimation>();
        if (ghostAnimation != null)
        {
            Destroy(ghostAnimation);
        }

        CanvasGroup ghostCanvasGroup = ghost.GetComponent<CanvasGroup>();
        if (ghostCanvasGroup == null)
        {
            ghostCanvasGroup = ghost.AddComponent<CanvasGroup>();
        }

        ghostCanvasGroup.blocksRaycasts = false;
        ghostCanvasGroup.interactable = false;

        RectTransform ghostRect = ghost.GetComponent<RectTransform>();
        ghostRect.position = originalRect.position;
        ghostRect.rotation = originalRect.rotation;
        ghostRect.localScale = originalRect.lossyScale;

        Canvas ghostCanvas = ghost.GetComponent<Canvas>();
        if (ghostCanvas == null)
        {
            ghostCanvas = ghost.AddComponent<Canvas>();
        }

        ghostCanvas.overrideSorting = true;
        ghostCanvas.sortingOrder = GetNextGhostSortingOrder();

        Vector3 centerWorldPos = GetCanvasWorldPoint(
            rootCanvas,
            new Vector2(Screen.width * 0.55f, Screen.height * 0.6f)
        );

        int burstIndex = GetBurstIndex();
        Vector3 burstOffset = new Vector3(
            burstIndex * burstIndentX,
            burstIndex * burstIndentY,
            0f
        );
        Vector3 centerTarget = centerWorldPos + burstOffset;

        Sequence seq = DOTween.Sequence();

        seq.Append(ghostRect.DOMove(centerTarget, flyToCenterDuration).SetEase(Ease.OutCubic));
        seq.Join(ghostRect.DOScale(centerScale, flyToCenterDuration).SetEase(Ease.OutBack));

        seq.AppendInterval(centerPauseDuration);

        seq.Append(ghostRect.DOMove(discardIcon.position, flyToDiscardDuration).SetEase(Ease.InBack));
        seq.Join(ghostRect.DOScale(discardScale, flyToDiscardDuration).SetEase(Ease.InBack));
        seq.Join(
            ghostRect.DORotate(
                new Vector3(0f, 0f, discardRotateAmount),
                flyToDiscardDuration,
                RotateMode.FastBeyond360
            )
        );

        seq.OnComplete(() =>
        {
            Destroy(ghost);
        });

        return seq;
    }
    private Vector3 GetCanvasWorldPoint(Canvas rootCanvas, Vector2 screenPoint)
    {
        RectTransform canvasRect = rootCanvas.transform as RectTransform;
        Camera cam = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : rootCanvas.worldCamera;

        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            canvasRect,
            screenPoint,
            cam,
            out Vector3 worldPoint
        );

        return worldPoint;
    }
    public Sequence DiscardAnimation(Canvas rootCanvas, RectTransform discardIcon, int batchIndex)
    {
        if (rootCanvas == null || discardIcon == null)
        {
            Debug.LogWarning("Missing rootCanvas or discardIcon for discard animation.");
            return null;
        }

        RectTransform originalRect = GetComponent<RectTransform>();

        GameObject ghost = Instantiate(gameObject, rootCanvas.transform);
        ghost.name = $"{gameObject.name}_DiscardGhost";

        CardView ghostCardView = ghost.GetComponent<CardView>();
        if (ghostCardView != null)
        {
            Destroy(ghostCardView);
        }

        CardAnimation ghostAnimation = ghost.GetComponent<CardAnimation>();
        if (ghostAnimation != null)
        {
            Destroy(ghostAnimation);
        }

        CanvasGroup ghostCanvasGroup = ghost.GetComponent<CanvasGroup>();
        if (ghostCanvasGroup == null)
        {
            ghostCanvasGroup = ghost.AddComponent<CanvasGroup>();
        }

        ghostCanvasGroup.blocksRaycasts = false;
        ghostCanvasGroup.interactable = false;

        RectTransform ghostRect = ghost.GetComponent<RectTransform>();
        ghostRect.position = originalRect.position;
        ghostRect.rotation = originalRect.rotation;
        ghostRect.localScale = originalRect.lossyScale;

        Canvas ghostCanvas = ghost.GetComponent<Canvas>();
        if (ghostCanvas == null)
        {
            ghostCanvas = ghost.AddComponent<Canvas>();
        }

        ghostCanvas.overrideSorting = true;
        ghostCanvas.sortingOrder = GetNextGhostSortingOrder();

        int burstIndex = Mathf.Clamp(batchIndex, 0, maxBurstIndentSteps);

        Vector3 startPos = ghostRect.position;
        Vector3 endPos = discardIcon.position;

        Vector3 burstOffset = new Vector3(
            burstIndex * burstIndentX,
            burstIndex * burstIndentY,
            0f
        );

        Vector3 launchPos = startPos + burstOffset * 0.35f;
        Vector3 midPoint = (startPos + endPos) * 0.5f;
        Vector3 controlPoint = midPoint + Vector3.up * 120f + burstOffset;

        Vector3[] path = new Vector3[]
        {
            launchPos,
            controlPoint,
            endPos
        };

        float startDelay = burstIndex * 0.03f;

        Sequence seq = DOTween.Sequence();

        seq.AppendInterval(startDelay);

        seq.Append(ghostRect.DOMove(launchPos, 0.06f).SetEase(Ease.OutQuad));
        seq.Join(ghostRect.DOScale(1.05f, 0.06f).SetEase(Ease.OutQuad));

        seq.Append(
            ghostRect.DOPath(path, flyToDiscardDuration, PathType.CatmullRom)
                .SetEase(Ease.InCubic)
        );

        seq.Join(
            ghostRect.DOScale(discardScale, flyToDiscardDuration)
                .SetEase(Ease.InBack)
        );

        seq.Join(
            ghostRect.DORotate(
                new Vector3(0f, 0f, burstIndex % 2 == 0 ? -discardRotateAmount : discardRotateAmount),
                flyToDiscardDuration,
                RotateMode.Fast
            ).SetEase(Ease.OutQuad)
        );

        seq.Join(
            ghostCanvasGroup.DOFade(0.85f, flyToDiscardDuration * 0.7f).From(1f)
        );

        seq.OnComplete(() =>
        {
            Destroy(ghost);
        });

        return seq;
    }

    /// <summary>
    /// Regular discard animation.
    /// Spawns a visual ghost of this card and animates it into the discard pile.
    /// Returns the tween sequence so callers can wait for completion.
    /// </summary>
    public Sequence DiscardAnimation(Canvas rootCanvas, RectTransform discardIcon)
    {
        return DiscardAnimation(rootCanvas, discardIcon, GetBurstIndex());
    }
}
