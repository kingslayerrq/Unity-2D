using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// The combat queue is plain C#, so these run with no scene and no frames.
/// </summary>
public sealed class CombatActionQueueTests
{
    private sealed class LambdaAction : ICombatAction
    {
        private readonly string _name;
        private readonly Action _body;

        public LambdaAction(string name, Action body)
        {
            _name = name;
            _body = body;
        }

        public void Execute() => _body?.Invoke();
        public string GetActionDetails() => _name;
    }

    private sealed class NamedEvent : ICombatEvent
    {
        public string Name { get; }

        public NamedEvent(string name)
        {
            Name = name;
        }
    }

    private CombatActionQueue queue;
    private List<string> log;

    [SetUp]
    public void SetUp()
    {
        queue = new CombatActionQueue { LogResolvedActions = false };
        log = new List<string>();
    }

    private LambdaAction Record(string name)
    {
        return new LambdaAction(name, () => log.Add(name));
    }

    [Test]
    public void QueuedActionResolvesBeforeEnqueueReturns()
    {
        queue.Enqueue(Record("a"), null);

        CollectionAssert.AreEqual(new[] { "a" }, log);
        Assert.That(queue.IsIdle, Is.True);
    }

    [Test]
    public void ActionsQueuedByAnActionRunAfterItInOrder()
    {
        queue.Enqueue(
            new LambdaAction("root", () =>
            {
                log.Add("root");
                queue.Enqueue(Record("first"), null);
                queue.Enqueue(
                    new LambdaAction("second", () =>
                    {
                        log.Add("second");
                        queue.Enqueue(Record("third"), null);
                    }),
                    null);
                log.Add("root-end");
            }),
            null);

        CollectionAssert.AreEqual(
            new[] { "root", "root-end", "first", "second", "third" },
            log);
        Assert.That(queue.IsIdle, Is.True);
    }

    [Test]
    public void CallbackRunsAfterItsActionAndBeforeTheNext()
    {
        queue.AutoDrain = false;
        queue.Enqueue(Record("a"), () => log.Add("a-done"));
        queue.Enqueue(Record("b"), null);

        queue.Drain();

        CollectionAssert.AreEqual(new[] { "a", "a-done", "b" }, log);
    }

    [Test]
    public void ExceptionInAnActionIsLoggedAndTheQueueContinues()
    {
        LogAssert.Expect(LogType.Exception, new Regex("boom"));

        queue.AutoDrain = false;
        queue.Enqueue(
            new LambdaAction(
                "bad",
                () => throw new InvalidOperationException("boom")),
            () => log.Add("bad-done"));
        queue.Enqueue(Record("after"), null);

        queue.Drain();

        CollectionAssert.AreEqual(new[] { "bad-done", "after" }, log);
        Assert.That(queue.IsIdle, Is.True);
    }

    [Test]
    public void SuspendedQueueWaitsUntilResumed()
    {
        queue.AutoDrain = false;
        queue.Enqueue(
            new LambdaAction("ask", () =>
            {
                log.Add("ask");
                queue.Suspend();
            }),
            null);
        queue.Enqueue(Record("after"), null);

        queue.Drain();

        CollectionAssert.AreEqual(new[] { "ask" }, log);
        Assert.That(queue.IsSuspended, Is.True);
        Assert.That(queue.Count, Is.EqualTo(1));

        queue.AutoDrain = true;
        queue.Resume();

        CollectionAssert.AreEqual(new[] { "ask", "after" }, log);
        Assert.That(queue.IsIdle, Is.True);
    }

    [Test]
    public void EventsArePublishedInTheOrderTheyHappen()
    {
        List<string> seen = new List<string>();
        queue.EventPublished +=
            combatEvent => seen.Add(((NamedEvent)combatEvent).Name);

        queue.AutoDrain = false;
        queue.Enqueue(
            new LambdaAction("a", () =>
            {
                queue.Publish(new NamedEvent("a-now"));
                queue.Enqueue(
                    new PublishEventAction(
                        queue,
                        new NamedEvent("a-later")),
                    null);
            }),
            null);
        queue.Enqueue(
            new LambdaAction(
                "b",
                () => queue.Publish(new NamedEvent("b-now"))),
            null);

        queue.Drain();

        CollectionAssert.AreEqual(
            new[] { "a-now", "b-now", "a-later" },
            seen);
    }

    [Test]
    public void FailingListenerDoesNotInterruptTheAction()
    {
        LogAssert.Expect(LogType.Exception, new Regex("listener"));

        queue.EventPublished +=
            _ => throw new InvalidOperationException("listener");

        queue.Enqueue(
            new LambdaAction("a", () =>
            {
                queue.Publish(new NamedEvent("x"));
                log.Add("after-publish");
            }),
            null);

        CollectionAssert.AreEqual(new[] { "after-publish" }, log);
    }

    [Test]
    public void WithoutAutoDrainNothingRunsUntilStepped()
    {
        int notifications = 0;
        queue.AutoDrain = false;
        queue.WorkAvailable += () => notifications++;

        queue.Enqueue(Record("a"), null);

        Assert.That(log, Is.Empty);
        Assert.That(notifications, Is.EqualTo(1));

        Assert.That(queue.Step(), Is.True);
        CollectionAssert.AreEqual(new[] { "a" }, log);
        Assert.That(queue.Step(), Is.False);
    }
}
