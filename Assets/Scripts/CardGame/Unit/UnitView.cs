using UnityEngine;
using UnityEngine.UI;

public class UnitView : MonoBehaviour
{
    public BaseUnitInstance UnitInstance { get; private set; }
    private UnitHealthbar _unitHealthbar;
    private UnitHealthText _unitHealthText;
    private UnitBlockIcon _unitBlockIcon;
    private UnitBlockText _unitBlockText;
    private UnitTarget _unitTarget;
    private UnitHighlight _unitHighlight;
    private EndTurnButton _endTurnButton;

    [SerializeField] private Image unitImage;
    [SerializeField] private BuffListView buffListView;

    private void Awake()
    {
        _unitHealthbar = GetComponentInChildren<UnitHealthbar>();
        _unitHealthText = GetComponentInChildren<UnitHealthText>();
        _unitBlockIcon = GetComponentInChildren<UnitBlockIcon>();
        _unitBlockText = GetComponentInChildren<UnitBlockText>();
        _unitTarget = GetComponent<UnitTarget>();
        _unitHighlight = GetComponentInChildren<UnitHighlight>();
        _endTurnButton = GetComponent<EndTurnButton>();
        if (unitImage == null) unitImage = GetComponentInChildren<Image>();
    }

    public void Initialize(BaseUnitInstance unitInstance)
    {
        UnitInstance = unitInstance;
        unitInstance.OnDisposed += OnUnitDisposed;

        if (_unitHealthbar != null) _unitHealthbar.Initialize(unitInstance);
        if (_unitHealthText != null) _unitHealthText.Initialize(unitInstance);
        if (_unitBlockIcon != null) _unitBlockIcon.Initialize(unitInstance);
        if (_unitBlockText != null) _unitBlockText.Initialize(unitInstance);
        if (_unitTarget != null) _unitTarget.Initialize(unitInstance);
        if (_unitHighlight != null) _unitHighlight.Initialize(unitInstance);
        if (_endTurnButton != null && unitInstance is BasePlayerUnitInstance playerInstance)
            _endTurnButton.Initialize(playerInstance);
        if (buffListView != null)
            buffListView.Initialize(unitInstance.BuffManager);

        if (unitImage != null && unitInstance.CurrentSprite != null)
        {
            unitImage.sprite = unitInstance.CurrentSprite;
            var r = unitInstance.CurrentSprite.rect;
            GetComponent<RectTransform>().sizeDelta = new Vector2(r.width, r.height);
        }

    }

    private void OnUnitDisposed()
    {
        if (UnitInstance != null)
            UnitInstance.OnDisposed -= OnUnitDisposed;
        Destroy(gameObject);
    }
}
