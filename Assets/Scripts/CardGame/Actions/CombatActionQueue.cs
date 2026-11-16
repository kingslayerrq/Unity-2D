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
        try
        {
            while (combatActionQueue.Count > 0)
            {
                QueuedAction current = combatActionQueue.Dequeue();

                // Step the action by hand so an exception inside it is logged
                // and skipped instead of killing the queue for the whole combat.
                IEnumerator runner = RunSafely(current.Action);
                while (runner.MoveNext())
                {
                    yield return runner.Current;
                }

                try
                {
                    Debug.Log("Resolved " + current.Action.GetActionDetails());
                    current.Callback?.Invoke();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
        }
        finally
        {
            IsResolvingAction = false;
        }
    }

    /// <summary>
    /// Runs an action and any nested enumerators it yields, catching exceptions
    /// from every step. Never throws.
    /// </summary>
    private static IEnumerator RunSafely(ICombatAction action)
    {
        Stack<IEnumerator> stack = new Stack<IEnumerator>();

        try
        {
            IEnumerator root = action.ExecuteAction();
            if (root != null) stack.Push(root);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }

        while (stack.Count > 0)
        {
            IEnumerator top = stack.Peek();
            bool moved = false;
            bool failed = false;

            try
            {
                moved = top.MoveNext();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                failed = true;
            }

            if (failed) break;

            if (!moved)
            {
                stack.Pop();
                continue;
            }

            object current = top.Current;
            if (current is IEnumerator nested)
            {
                // Nested coroutines and CustomYieldInstructions (WaitUntil) are
                // stepped here so their exceptions are caught too.
                stack.Push(nested);
                continue;
            }

            yield return current;
        }
    }
}
