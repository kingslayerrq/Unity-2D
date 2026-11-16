using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The ordered queue of combat logic, and the stream of events that logic emits.
///
/// This class is plain C#: it needs no scene, coroutine or frame.
///
/// By default (<see cref="AutoDrain"/>) every action runs as soon as it is
/// queued, so a call such as playing a card returns only once everything it
/// caused has resolved. Tests and bots use this mode.
///
/// A presentation layer can instead turn <see cref="AutoDrain"/> off and drive
/// the queue one <see cref="Step"/> at a time, playing the animations for the
/// events each step published before asking for the next one.
/// </summary>
public class CombatActionQueue
{
    private class QueuedAction {
        public ICombatAction Action;
        public Action Callback;
    }

    // Guards a bot or test run against an action that keeps re-queueing itself.
    private const int MaxStepsPerDrain = 100000;

    private readonly Queue<QueuedAction> combatActionQueue = new Queue<QueuedAction>();

    private bool isStepping;
    private bool isDraining;

    /// <summary>
    /// When true, queued actions run immediately and to completion.
    /// When false, something else must call <see cref="Step"/>.
    /// </summary>
    public bool AutoDrain { get; set; } = true;

    /// <summary>Logs every resolved action. Turn off for bulk headless runs.</summary>
    public bool LogResolvedActions { get; set; } = true;

    /// <summary>True while the queue is waiting for a choice to be made.</summary>
    public bool IsSuspended { get; private set; }

    public bool IsResolvingAction => isStepping || isDraining;
    public int Count => combatActionQueue.Count;

    /// <summary>Nothing queued, nothing running, nothing awaited.</summary>
    public bool IsIdle => combatActionQueue.Count == 0 && !isStepping && !IsSuspended;

    /// <summary>Raised, in order, for everything combat logic publishes.</summary>
    public event Action<ICombatEvent> EventPublished;

    /// <summary>
    /// Raised when there may be something to step and <see cref="AutoDrain"/>
    /// is off: an action was queued, or the queue was resumed.
    /// </summary>
    public event Action WorkAvailable;

    public void Enqueue(ICombatAction action, Action onComplete)
    {
        if (action == null) return;
        combatActionQueue.Enqueue(new QueuedAction { Action = action, Callback = onComplete });
        NotifyWork();
    }

    public void Clear()
    {
        combatActionQueue.Clear();
    }

    /// <summary>
    /// Runs the next action and then its completion callback.
    /// </summary>
    /// <returns>
    /// False if nothing ran: the queue is empty, suspended, or already inside a step.
    /// </returns>
    public bool Step()
    {
        if (isStepping || IsSuspended || combatActionQueue.Count == 0) return false;

        isStepping = true;
        try
        {
            QueuedAction current = combatActionQueue.Dequeue();

            // An exception inside one action is logged and skipped. It must
            // not stop the queue, or the whole combat would freeze.
            try
            {
                current.Action.Execute();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }

            if (LogResolvedActions)
            {
                try
                {
                    Debug.Log("Resolved " + current.Action.GetActionDetails());
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }

            try
            {
                current.Callback?.Invoke();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
        finally
        {
            isStepping = false;
        }

        return true;
    }

    /// <summary>
    /// Steps until the queue is empty or suspended. Does nothing when called
    /// from inside an action, because the outer drain is already running.
    /// </summary>
    public void Drain()
    {
        if (isDraining || isStepping) return;

        isDraining = true;
        try
        {
            int steps = 0;
            while (Step())
            {
                if (++steps < MaxStepsPerDrain) continue;

                Debug.LogError(
                    $"CombatActionQueue stopped after {MaxStepsPerDrain} steps in one drain. " +
                    "An action is probably re-queueing itself forever.");
                break;
            }
        }
        finally
        {
            isDraining = false;
        }
    }

    /// <summary>
    /// Pauses the queue after the current action. Used while a choice is
    /// pending; call <see cref="Resume"/> once it has been made.
    /// </summary>
    public void Suspend()
    {
        IsSuspended = true;
    }

    public void Resume()
    {
        if (!IsSuspended) return;

        IsSuspended = false;
        NotifyWork();
    }

    /// <summary>
    /// Tells listeners that something happened. A failing listener is logged
    /// and never interrupts the logic that published the event.
    /// </summary>
    public void Publish(ICombatEvent combatEvent)
    {
        if (combatEvent == null) return;

        try
        {
            EventPublished?.Invoke(combatEvent);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    private void NotifyWork()
    {
        if (AutoDrain)
        {
            Drain();
        }
        else
        {
            WorkAvailable?.Invoke();
        }
    }
}
