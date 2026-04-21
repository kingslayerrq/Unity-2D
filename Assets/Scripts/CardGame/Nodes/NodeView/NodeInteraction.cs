using System;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

[RequireComponent(typeof(NodeView))]
public class NodeInteraction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private NodeView _view;
    private void Awake()
    {
        _view = GetComponent<NodeView>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _view.VisualHoverEnter();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _view.VisualHoverExit();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_view != null && _view.Data.RuntimeNode != null && _view.Data.RuntimeNode.IsNext)
        {
            Debug.Log($"Click {_view.name}");
            RunManager.Instance.EnterNode(_view.Data.RuntimeNode);
        }
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }
}
