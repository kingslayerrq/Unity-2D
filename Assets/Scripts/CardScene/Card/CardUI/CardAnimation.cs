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
    
    [SerializeField] private const int GhostSortingStart = 1500;
    private static int nextGhostSortingOrder = GhostSortingStart;
    
    private static float lastGhostSpawnTime = -999f;
    private static int currentBurstIndex = 0;

    private static int GetNextGhostSortingOrder()
    {
        // earlier spawned card stays on top
        if (nextGhostSortingOrder < 1000)
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
    
    public Sequence DiscardAnimationSelf(Canvas rootCanvas, RectTransform discardIcon, int batchIndex)
    {
        if (rootCanvas == null || discardIcon == null)
        {
            Debug.LogWarning("Missing rootCanvas or discardIcon for discard animation.");
            return null;
        }

        RectTransform cardRect = GetComponent<RectTransform>();
        if (cardRect == null)
        {
            Debug.LogWarning("Card is missing RectTransform.");
            return null;
        }

        // --- Prepare Actual Card for Animation ---
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Prevent interaction during animation
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        // Stop layout control so it can fly freely
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
        // -----------------------------------------

        int burstIndex = Mathf.Clamp(batchIndex, 0, maxBurstIndentSteps);

        Vector3 startPos = cardRect.position;
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
        
        // Bind the sequence to this GameObject so it safely auto-kills if the card is destroyed
        seq.SetLink(gameObject); 

        seq.AppendInterval(startDelay);

        seq.Append(cardRect.DOMove(launchPos, 0.06f).SetEase(Ease.OutQuad));
        seq.Join(cardRect.DOScale(1.05f, 0.06f).SetEase(Ease.OutQuad));

        seq.Append(
            cardRect.DOPath(path, flyToDiscardDuration, PathType.CatmullRom)
                .SetEase(Ease.InCubic)
        );

        seq.Join(
            cardRect.DOScale(discardScale, flyToDiscardDuration)
                .SetEase(Ease.InBack)
        );

        seq.Join(
            cardRect.DORotate(
                new Vector3(0f, 0f, burstIndex % 2 == 0 ? -discardRotateAmount : discardRotateAmount),
                flyToDiscardDuration,
                RotateMode.Fast
            ).SetEase(Ease.OutQuad)
        );

        // Fade the actual card out slightly as it reaches the pile
        seq.Join(
            canvasGroup.DOFade(0.85f, flyToDiscardDuration * 0.7f).From(1f)
        );

        // Removed the OnComplete Destroy(ghost) block, because we want the card to stay alive 
        // so PlayerHandView can call CompleteAnimateOut and send it to the pool!

        return seq;
    }
   
    
}
