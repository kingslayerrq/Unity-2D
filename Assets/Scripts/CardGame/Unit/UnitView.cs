using UnityEngine;
using UnityEngine.UI;

public class UnitView : MonoBehaviour
{
    public BaseUnitInstance UnitInstance { get; private set; }
    private UnitHealthbar _unitHealthbar;
    private UnitHealthText _unitHealthText;
    private UnitTarget _unitTarget;
    private UnitHighlight _unitHighlight;
    private EndTurnButton _endTurnButton;
    private BuffManager _buffManager;

    [SerializeField] private Image unitImage;
    [SerializeField] private BuffListView buffListView;

    private void Awake()
    {
        _unitHealthbar = GetComponentInChildren<UnitHealthbar>();
        _unitHealthText = GetComponentInChildren<UnitHealthText>();
        _unitTarget = GetComponent<UnitTarget>();
        _unitHighlight = GetComponentInChildren<UnitHighlight>();
        _endTurnButton = GetComponent<EndTurnButton>();
        _buffManager = GetComponent<BuffManager>();
        if (unitImage == null) unitImage = GetComponentInChildren<Image>();
    }

    public void Initialize(BaseUnitInstance unitInstance)
    {
        UnitInstance = unitInstance;
        unitInstance.OnDisposed += OnUnitDisposed;

        if (_unitHealthbar != null) _unitHealthbar.Initialize(unitInstance);
        if (_unitHealthText != null) _unitHealthText.Initialize(unitInstance);
        if (_unitTarget != null) _unitTarget.Initialize(unitInstance);
        if (_unitHighlight != null) _unitHighlight.Initialize(unitInstance);
        if (_endTurnButton != null && unitInstance is BasePlayerUnitInstance playerInstance)
            _endTurnButton.Initialize(playerInstance);
        if (buffListView != null && _buffManager != null)
            buffListView.Initialize(_buffManager);

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
