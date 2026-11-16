using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Checks how the runner paces the queue against presentation. The routine a
/// coroutine would run is stepped by hand, one <see cref="ManualCoroutine.Tick"/>
/// per frame, so no scene or play mode is needed.
/// </summary>
public sealed class CombatQueueRunnerTests
{
    private sealed class ManualCoroutine
    {
        private IEnumerator routine;

        public bool IsRunning => routine != null;

        // Like StartCoroutine: runs up to the first wait straight away.
        public void Start(IEnumerator newRoutine)
        {
            routine = newRoutine;
            Tick();
        }

        public void Tick()
        {
            if (routine != null && !routine.MoveNext())
                routine = null;
        }
    }

    private sealed class LambdaAction : ICombatAction
    {
        private readonly Action _body;

        public LambdaAction(Action body)
        {
            _body = body;
        }

        public void Execute() => _body?.Invoke();
        public string GetActionDetails() => "LambdaAction";
    }

    private sealed class NamedEvent : ICombatEvent
    {
        public string Name { get; }

        public NamedEvent(string name)
        {
            Name = name;
        }
    }

    private sealed class LambdaPresenter : ICombatEventPresenter
    {
        private readonly Func<ICombatEvent, IEnumerator> _present;

        public LambdaPresenter(Func<ICombatEvent, IEnumerator> present)
        {
            _present = present;
        }

        public IEnumerator Present(ICombatEvent combatEvent) =>
            _present(combatEvent);
    }

    private CombatActionQueue queue;
    private ManualCoroutine coroutine;
    private CombatQueueRunner runner;
    private List<string> log;

    [SetUp]
    public void SetUp()
    {
        queue = new CombatActionQueue { LogResolvedActions = false };
        coroutine = new ManualCoroutine();
        runner = new CombatQueueRunner(queue, coroutine.Start);
        log = new List<string>();
    }

    private LambdaAction Record(string name)
    {
        return new LambdaAction(() => log.Add(name));
    }

    private IEnumerator Animation(string name, int frames)
    {
        log.Add(name + "-start");

        for (int i = 0; i < frames; i++)
        {
            yield return null;
        }

        log.Add(name + "-end");
    }

    [Test]
    public void FirstActionRunsInsideTheEnqueueCall()
    {
        queue.Enqueue(Record("a"), null);

        CollectionAssert.AreEqual(new[] { "a" }, log);
    }

    [Test]
    public void ActionsWithNothingToShowRunOnePerFrame()
    {
        queue.Enqueue(Record("a"), null);
        queue.Enqueue(Record("b"), null);

        CollectionAssert.AreEqual(new[] { "a" }, log);

        coroutine.Tick();

        CollectionAssert.AreEqual(new[] { "a", "b" }, log);

        coroutine.Tick();

        Assert.That(coroutine.IsRunning, Is.False);
    }

    [Test]
    public void NextActionWaitsForThePresentationToFinish()
    {
        runner.AddPresenter(
            new LambdaPresenter(
                combatEvent =>
                    Animation(((NamedEvent)combatEvent).Name, 2)));

        queue.Enqueue(
            new LambdaAction(() =>
            {
                log.Add("a");
                queue.Publish(new NamedEvent("anim"));
            }),
            null);
        queue.Enqueue(Record("b"), null);

        CollectionAssert.AreEqual(new[] { "a", "anim-start" }, log);

        coroutine.Tick();

        CollectionAssert.AreEqual(new[] { "a", "anim-start" }, log);

        coroutine.Tick();

        CollectionAssert.AreEqual(
            new[] { "a", "anim-start", "anim-end", "b" },
            log);
    }

    [Test]
    public void InstantPresentationHappensInsideTheAction()
    {
        runner.AddPresenter(
            new LambdaPresenter(combatEvent =>
            {
                log.Add("shown");
                return null;
            }));

        queue.Enqueue(
            new LambdaAction(() =>
            {
                queue.Publish(new NamedEvent("x"));
                log.Add("action-end");
            }),
            null);

        CollectionAssert.AreEqual(
            new[] { "shown", "action-end" },
            log);
    }

    [Test]
    public void SuspendedQueueStopsTheRunnerUntilResumed()
    {
        queue.Enqueue(
            new LambdaAction(() =>
            {
                log.Add("ask");
                queue.Suspend();
            }),
            null);
        queue.Enqueue(Record("after"), null);

        coroutine.Tick();

        CollectionAssert.AreEqual(new[] { "ask" }, log);
        Assert.That(coroutine.IsRunning, Is.False);

        queue.Resume();

        CollectionAssert.AreEqual(new[] { "ask", "after" }, log);
    }

    [Test]
    public void FailingPresenterDoesNotStopCombat()
    {
        LogAssert.Expect(LogType.Exception, new Regex("view broke"));
        LogAssert.Expect(LogType.Exception, new Regex("animation broke"));

        runner.AddPresenter(
            new LambdaPresenter(
                _ => throw new InvalidOperationException("view broke")));
        runner.AddPresenter(
            new LambdaPresenter(_ => BrokenAnimation()));

        queue.Enqueue(
            new LambdaAction(() =>
            {
                log.Add("a");
                queue.Publish(new NamedEvent("x"));
            }),
            null);
        queue.Enqueue(Record("b"), null);

        for (int frame = 0; frame < 5 && coroutine.IsRunning; frame++)
        {
            coroutine.Tick();
        }

        CollectionAssert.AreEqual(new[] { "a", "b" }, log);
        Assert.That(coroutine.IsRunning, Is.False);
    }

    [Test]
    public void StoppedRunnerHandsTheQueueBackToImmediateResolution()
    {
        runner.Stop();

        queue.Enqueue(Record("a"), null);
        queue.Enqueue(Record("b"), null);

        CollectionAssert.AreEqual(new[] { "a", "b" }, log);
        Assert.That(coroutine.IsRunning, Is.False);
    }

    private static IEnumerator BrokenAnimation()
    {
        yield return null;
        throw new InvalidOperationException("animation broke");
    }
}
