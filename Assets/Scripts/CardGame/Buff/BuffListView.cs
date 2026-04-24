using System.Collections.Generic;
using UnityEngine;

public class BuffListView : MonoBehaviour
{
    [SerializeField] private BuffView buffViewPrefab;
    [SerializeField] private Transform container;

    private BuffManager _buffManager;
    private readonly List<BuffView> _views = new();

    public void Initialize(BuffManager buffManager)
    {
        _buffManager = buffManager;
        _buffManager.OnBuffsChanged += Refresh;
    }

    private void Refresh()
    {
        var buffs = _buffManager.ActiveBuffs;
        int prevCount = _views.Count;

        while (_views.Count > buffs.Count)
        {
            Destroy(_views[^1].gameObject);
            _views.RemoveAt(_views.Count - 1);
        }

        while (_views.Count < buffs.Count)
            _views.Add(Instantiate(buffViewPrefab, container));

        for (int i = 0; i < buffs.Count; i++)
        {
            if (i < prevCount)
                _views[i].Refresh(buffs[i]);
            else
                _views[i].Initialize(buffs[i]);
        }
    }

    private void OnDestroy()
    {
        if (_buffManager != null)
            _buffManager.OnBuffsChanged -= Refresh;
    }
}
