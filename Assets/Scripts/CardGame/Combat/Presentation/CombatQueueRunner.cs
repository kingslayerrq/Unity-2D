using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Drives a <see cref="CombatActionQueue"/> at the pace of the screen.
///
/// The queue itself knows nothing about frames or animations. This runner
/// steps it one action at a time and, between steps, plays whatever the
/// presenters returned for the events that step published. Combat logic and
/// what the player sees therefore stay in the same order.
///
/// Without a runner the queue resolves everything immediately, which is how
/// tests and bots use it.
/// </summary>
public sealed class CombatQueueRunner
{
    private readonly CombatActionQueue _queue;
    private readonly Func<MonoBehaviour> _coroutineHost;
    private readonly Action<IEnumerator> _manualStart;
    private readonly List<ICombatEventPresenter> _presenters = new List<ICombatEventPresenter>();

    // Routines returned by presenters, waiting to be played in order.
    private readonly Queue<IEnumerator> _pendingPresentations = new Queue<IEnumerator>();

    private MonoBehaviour _activeHost;
    private bool _isRunning;
    private bool _isStopped;

    /// <param name="queue">The queue to drive. Its AutoDrain is turned off.</param>
    /// <param name="coroutineHost">
    /// Returns the object to run coroutines on. Asked each time work starts,
    /// so it may return a different host over time.
    /// </param>
    public CombatQueueRunner(CombatActionQueue queue, Func<MonoBehaviour> coroutineHost)
        : this(queue, coroutineHost, null)
    {
    }

    /// <summary>
    /// For tests: instead of starting a coroutine, hands the routine to
    /// <paramref name="manualStart"/>, which steps it by hand.
    /// </summary>
    internal CombatQueueRunner(CombatActionQueue queue, Action<IEnumerator> manualStart)
        : this(queue, null, manualStart)
    {
    }

    private CombatQueueRunner(CombatActionQueue queue, Func<MonoBehaviour> coroutineHost,
        Action<IEnumerator> manualStart)
    {
        _queue = queue;
        _coroutineHost = coroutineHost;
        _manualStart = manualStart;

        _queue.AutoDrain = false;
        _queue.EventPublished += HandleEventPublished;
        _queue.WorkAvailable += StartIfIdle;
    }

    public void AddPresenter(ICombatEventPresenter presenter)
    {
        if (presenter != null && !_presenters.Contains(presenter))
            _presenters.Add(presenter);
    }

    /// <summary>
    /// Stops driving the queue and hands it back to immediate resolution.
    /// Call when the views this runner feeds are going away.
    /// </summary>
    public void Stop()
    {
        if (_isStopped) return;

        _isStopped = true;
        _queue.EventPublished -= HandleEventPublished;
        _queue.WorkAvailable -= StartIfIdle;
        _pendingPresentations.Clear();
        _queue.AutoDrain = true;
    }

    private void HandleEventPublished(ICombatEvent combatEvent)
    {
        for (int i = 0; i < _presenters.Count; i++)
        {
            try
            {
                IEnumerator presentation = _presenters[i].Present(combatEvent);
                if (presentation != null) _pendingPresentations.Enqueue(presentation);
            }
            catch (Exception exception)
            {
                // A broken view must never stop combat.
                Debug.LogException(exception);
            }
        }

        // Covers an event published outside a step, such as from setup code.
        if (_pendingPresentations.Count > 0) StartIfIdle();
    }

    private void StartIfIdle()
    {
        if (_isStopped) return;

        if (_manualStart != null)
        {
            if (!_isRunning) _manualStart(Run());
            return;
        }

        // Unity drops a coroutine without unwinding it when its host is
        // destroyed or disabled. If that happened, start again on a new host.
        bool hostLost = _activeHost == null || !_activeHost.isActiveAndEnabled;
        if (_isRunning && !hostLost) return;

        MonoBehaviour host = _coroutineHost?.Invoke();
        if (host == null || !host.isActiveAndEnabled)
        {
            Debug.LogError("CombatQueueRunner has no active object to run coroutines on.");
            return;
        }

        // Runs synchronously up to its first wait, so an action queued while
        // idle still resolves within the call that queued it.
        _activeHost = host;
        host.StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        _isRunning = true;
        try
        {
            while (!_isStopped)
            {
                // Finish showing the previous step before running the next.
                while (!_isStopped && _pendingPresentations.Count > 0)
                {
                    IEnumerator presentation = RunSafely(_pendingPresentations.Dequeue());
                    while (presentation.MoveNext())
                    {
                        yield return presentation.Current;
                    }
                }

                if (_isStopped || !_queue.Step()) break;

                if (_pendingPresentations.Count == 0)
                {
                    // Nothing to show for that step. Leave one frame between
                    // actions so results still land one after another.
                    yield return null;
                }
            }
        }
        finally
        {
            _isRunning = false;
        }
    }

    /// <summary>
    /// Steps a routine and any routines nested inside it, catching exceptions
    /// from every step. Never throws.
    /// </summary>
    private static IEnumerator RunSafely(IEnumerator root)
    {
        Stack<IEnumerator> stack = new Stack<IEnumerator>();
        if (root != null) stack.Push(root);

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
                // Nested routines and CustomYieldInstructions (WaitUntil) are
                // stepped here so their exceptions are caught too.
                stack.Push(nested);
                continue;
            }

            yield return current;
        }
    }
}
