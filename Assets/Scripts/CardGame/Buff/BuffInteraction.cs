using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(BuffView))]
public class BuffInteraction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private BuffView _view;

    private void Awake()
    {
        _view = GetComponent<BuffView>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _view.VisualHoverEnter();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _view.VisualHoverExit();
    }
}
