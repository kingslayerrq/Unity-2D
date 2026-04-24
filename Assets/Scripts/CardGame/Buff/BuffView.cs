using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuffView : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI stackText;

    [SerializeField] private KeywordPopupView keywordPopupPrefab;
    [SerializeField] private Transform keywordPopupParent;
    private KeywordPopupView _keywordPopup;

    [Header("Animation")]
    private Vector3 _origScale;
    [SerializeField] private float hoverScale;
    [SerializeField] private float hoverAnimationTime;
    private Tween _hoverTween;

    private void Awake()
    {
        _origScale = transform.localScale;
    }

    public void Initialize(BuffInstance buffInstance)
    {
        if (icon == null)
        {
            Debug.LogError("BuffView: icon Image is not assigned.", this);
            return;
        }
        icon.sprite = buffInstance.Data.BuffIcon;
        icon.color = Color.white;
        UpdateStacks(buffInstance.Stacks);

        _keywordPopup = Instantiate(keywordPopupPrefab, keywordPopupParent);
        _keywordPopup.Setup(buffInstance.Data.KeywordData);
        _keywordPopup.Hide();
    }

    public void Refresh(BuffInstance buffInstance)
    {
        UpdateStacks(buffInstance.Stacks);
    }

    public void VisualHoverEnter()
    {
        _hoverTween?.Kill();
        _hoverTween = transform.DOScale(_origScale * hoverScale, hoverAnimationTime).SetEase(Ease.OutBack);
        _keywordPopup?.Show();
    }

    public void VisualHoverExit()
    {
        _hoverTween?.Kill();
        _hoverTween = transform.DOScale(_origScale, hoverAnimationTime).SetEase(Ease.OutQuad);
        _keywordPopup?.Hide();
    }

    private void UpdateStacks(int stacks)
    {
        stackText.text = stacks > 1 ? stacks.ToString() : string.Empty;
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }
}
