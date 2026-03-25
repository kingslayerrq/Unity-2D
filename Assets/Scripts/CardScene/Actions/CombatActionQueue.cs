using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatActionQueue 
{
    private readonly Queue<ICombatAction> combatActionQueue = new Queue<ICombatAction>();
    
    public bool IsResolvingAction { get; private set; }
    public int Count => combatActionQueue.Count;
    
    public void Enqueue(ICombatAction action)
    {
        if (action == null) return;
        combatActionQueue.Enqueue(action);
    }

    public void Clear()
    {
        combatActionQueue.Clear();
    }

    public void StartResolveAll(Action onComplete = null)
    {
        if (IsResolvingAction) return;
        CoroutineRunner.Instance.StartCoroutine(ResolveCoroutine(onComplete));
    }

    public IEnumerator ResolveCoroutine(Action onComplete)
    {
        IsResolvingAction = true;
        while (combatActionQueue.Count > 0)
        {
            Debug.Log("Resolving");
            ICombatAction action = combatActionQueue.Dequeue();
            yield return action.ExecuteAction();
        }
        IsResolvingAction = false;
        onComplete?.Invoke();
    }
}
