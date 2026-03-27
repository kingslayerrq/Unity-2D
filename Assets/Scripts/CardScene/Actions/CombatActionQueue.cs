using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatActionQueue 
{
    private class QueuedAction {
        public ICombatAction Action;
        public Action Callback;
    }
    private readonly Queue<QueuedAction> combatActionQueue = new Queue<QueuedAction>();
    
    public bool IsResolvingAction { get; private set; }
    public int Count => combatActionQueue.Count;
    
    public void Enqueue(ICombatAction action, Action onComplete)
    {
        if (action == null) return;
        combatActionQueue.Enqueue(new QueuedAction { Action = action, Callback = onComplete });
        if (!IsResolvingAction)
        {
            CoroutineRunner.Instance.StartCoroutine(ResolveCoroutine());
        }
    }

    public void Clear()
    {
        combatActionQueue.Clear();
    }
    

    public IEnumerator ResolveCoroutine()
    {
        IsResolvingAction = true;
        while (combatActionQueue.Count > 0)
        {
            QueuedAction current = combatActionQueue.Dequeue();
            yield return current.Action.ExecuteAction();
            Debug.Log("Resolved" + current.Action.GetActionDetails());
            current.Callback?.Invoke();
        }
        IsResolvingAction = false;
    }
}
