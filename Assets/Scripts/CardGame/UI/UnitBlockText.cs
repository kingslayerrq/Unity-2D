using TMPro;
using UnityEngine;

public class UnitBlockText : MonoBehaviour
{
    private TMP_Text _text;
    private BaseUnitInstance _unitInstance;
    private void Awake()
    {
        _text = GetComponent<TMP_Text>();

    }

    public void Initialize(BaseUnitInstance unitInstance)
    {
        if (_unitInstance != null)
        {
            _unitInstance.OnBlockChanged -= UpdateBlockText;
        }
        
        _unitInstance = unitInstance;

        if (_unitInstance != null)
        {
            _unitInstance.OnBlockChanged += UpdateBlockText;
            UpdateBlockText(_unitInstance.CurrentBlock);
        }
    }

    private void UpdateBlockText(int curBlock)
    {
        if (curBlock > 0)
        {
            if (!_text.enabled) _text.enabled = true;
            _text.text = curBlock.ToString();
        }
        else
        {
            _text.enabled = false;
        }
    }



    private void OnDestroy()
    {
        if (_unitInstance != null) _unitInstance.OnBlockChanged -= UpdateBlockText;
    }
}
