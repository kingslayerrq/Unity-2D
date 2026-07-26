using System;
using UnityEngine;
using UnityEngine.UI;

public class UnitBlockIcon : MonoBehaviour
{
    private Image _image;
    private BaseUnitInstance _unitInstance;
    
    private void Awake()
    {
        if (_image == null) _image = GetComponent<Image>();
    }
    
    public void Initialize(BaseUnitInstance unitInstance)
    {
        if (_unitInstance != null)
        {
            _unitInstance.OnBlockChanged -= UpdateBlockIcon;
        }
        _unitInstance = unitInstance;

        if (_unitInstance != null)
        {
            _unitInstance.OnBlockChanged += UpdateBlockIcon;
            UpdateBlockIcon(_unitInstance.CurrentBlock);
        }
    }

    private void UpdateBlockIcon(int curBlock)
    {
        if (curBlock > 0)
        {
            if (!_image.enabled) _image.enabled = true;
        }
        else
        {
            _image.enabled = false;
        }
    }

    private void OnDestroy()
    {
        if (_unitInstance == null) return;
        _unitInstance.OnBlockChanged -= UpdateBlockIcon;
    }
}
