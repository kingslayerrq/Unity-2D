# Architecture: rules and views

The card game is split in two. **Rules** are plain C# classes that hold the
state of a run and a combat and decide what happens. **Views** are
MonoBehaviours that show that state and pass the player's input back in.

The rules never know the screen exists. A whole run can be played with no
scene loaded, which is what the tests do and what a bot would do.

This guide is for adding features without breaking that split.

## Keep in mind

Before you write a new class, decide which side it is on.

**It is a rule if** it changes health, cards, energy, buffs, turns, the map,
rewards or anything else that affects the outcome. Then:

1. Make it a plain class. No `MonoBehaviour`, no `GameObject`, no
   `UnityEngine.UI`, no DOTween.
2. Never look anything up globally. No `RunManager.Instance`, no
   `GlobalProgressionManager.Instance`, no `FindFirstObjectByType`.
3. Never wait. No coroutines, no `yield`, no "when the animation finishes".
4. Never call a view. To show something, publish an event. To get a choice
   from the player, raise a decision.
5. Take random numbers only from the combat's or the run's own stream.

**It is a view if** it draws, animates, plays sound or handles input. Then:

1. Read the model and subscribe to its events. Do not keep your own copy of
   game state.
2. Do not decide anything. If a view needs to know whether something is
   allowed, ask the rules.
3. Change the game only by submitting a command, the same command a bot
   would submit.

A quick test: if deleting the class would change who wins, it is a rule.

## Where things live

All paths are under `Assets/Scripts/CardGame`.

### Rules (plain C#)

| What | Where |
|---|---|
| A run: seed, character, map, current node, rewards | `Run/RunSession.cs` |
| Run decisions and commands | `Run/RunDecisions.cs` |
| Map generation | `Map/MapBuilder.cs`, `Map/RunMap.cs`, `Map/MapNodeData.cs` |
| Reward rolling | `Run/CardRewardGenerator.cs` |
| Node contents | `Nodes/*.cs`, `Nodes/Config/*.cs` |
| A combat: turn flow, decisions, commands | `Managers/CombatManager.cs` |
| What each rule does (damage, block, draw, buffs) | `CombatSystem.cs` |
| The queue of things being resolved | `Actions/CombatActionQueue.cs`, `Actions/*Action.cs` |
| Combat decisions and commands | `Combat/Decisions`, `Combat/Commands` |
| What a card may target | `Combat/Targeting/CardTargeting.cs` |
| Events for the screen | `Combat/Events` |
| What a combat is given from outside | `Combat/CombatContext.cs` |
| Units, hand, piles, deck | `Unit/Base*Instance.cs`, `Unit/PlayerHand.cs`, `Managers/CardPileManager.cs`, `Deck` |
| Buffs | `Managers/BuffManager.cs`, `Buff/BuffInstance.cs`, `Buff/BuffInstances` |
| Card effects and conditions | `Card/CardEffects` |
| Enemy intents | `Unit/EnemyUnits/EnemyIntents` |

Data assets (`CardData`, `BuffData`, `EnemyUnitData`, node configs) are
ScriptableObjects. Rules read them; they hold no state that changes in play.

### Views (MonoBehaviours)

| What | Where |
|---|---|
| Shows a run: panels, scene loading, result screen | `Managers/RunManager.cs` |
| Draws the map | `Map/MapGenerator.cs`, `Nodes/NodeView` |
| Reward screen | `Managers/NodeRewardManager.cs` |
| Shows a combat and wires its screen | `Combat/Presentation/CombatPresenter.cs` |
| Spawns unit views | `Combat/Presentation/UnitSpawner.cs` |
| Pile counters | `Combat/Presentation/CardPilePresenter.cs`, `UI/*PileUI.cs` |
| Paces the queue to animations | `Combat/Presentation/CombatQueueRunner.cs` |
| Hand, cards, drag and drop | `PlayerHandView.cs`, `Card/CardUI` |
| Units, health, block, buffs | `Unit/UnitView.cs`, `UI/Unit*.cs`, `Buff/Buff*View.cs` |
| Card selection prompt | `UI/CardSelectionCanvas.cs` |
| Bridge to saved progression | `Combat/Presentation/GlobalProgressionRecorder.cs` |

## How the two sides talk

There are four channels. Use these and nothing else.

| Direction | Channel | Example |
|---|---|---|
| View reads rules | Properties on the model | `unit.CurrentHealth`, `combat.Enemies` |
| Rules tell view a value changed | C# events on the model | `unit.OnHealthChanged`, `buffManager.OnBuffsChanged` |
| Rules tell view something happened, in order | `ICombatEvent` published on the queue | `CardsDiscardedEvent` |
| View tells rules what the player wants | A command through `Submit` | `combat.Submit(new PlayCardCommand(card, targets))` |

When the rules need an answer from the player, they raise a **decision**
and stop until a command answers it. The decisions today are
`PlayerTurnDecision` and `CardChoiceDecision` in combat, and
`ChooseNodeDecision` and `CardRewardDecision` in the run. Read the current
one from `combat.PendingDecision` or `run.PendingDecision`.

### What happens when a card is played

1. `CardInteraction` (view) asks the drop receiver what the drop means. The
   receiver asks `CardTargeting`.
2. The view submits `PlayCardCommand`. `CombatManager.Submit` checks it is
   the player's turn, the card is affordable and the targets are legal. If
   not, it rejects the command and nothing changes.
3. Energy is spent and a `PlayCardAction` is queued.
4. The action publishes `CardsLeavingHandEvent`, then `CombatSystem`
   resolves the card. Each card effect queues its own actions (damage,
   block, draw).
5. Those actions resolve one by one. `CombatSystem` runs the buff hooks
   around each.
6. A `CardPlayResolvedEvent` is published last. `PlayerHandView` plays the
   fly-out animation.

In the game, `CombatQueueRunner` steps the queue one action at a time and
waits for that step's animations. With no runner attached, every step runs
immediately, so `Submit` returns once the whole card has resolved.

### Driving the game with no scene

This loop is the whole interface. Tests use it; a bot would too.

```csharp
RunSession run = new RunSession(seed, config, progress);
run.ChooseCharacter(character);

while (!run.IsOver)
{
    if (run.State == RunState.CombatState)
    {
        switch (run.Combat.PendingDecision)
        {
            case PlayerTurnDecision turn:
                List<PlayCardCommand> plays = turn.GetLegalPlays();
                if (plays.Count > 0) run.Combat.Submit(plays[0]);
                else run.Combat.Submit(new EndTurnCommand());
                break;
            case CardChoiceDecision choice:
                run.Combat.Submit(new ChooseCardsCommand(choice.GetMinimalPicks()));
                break;
        }
        continue;
    }

    switch (run.PendingDecision)
    {
        case ChooseNodeDecision choose:
            run.Submit(new ChooseNodeCommand(choose.Options[0]));
            break;
        case CardRewardDecision reward:
            run.Submit(new SkipRewardCommand());
            break;
        default:
            return; // nothing left to decide
    }
}
```

`Submit` returns a `CommandResult`. A rejected command changes nothing and
says why in `Reason`.

## Recipes

### A new card effect

1. Add a config in `Card/CardEffects/Configs`: a `[Serializable]` class
   deriving from `BaseCardEffectConfig`, with `[SerializeField]` fields.
2. Add the effect in `Card/CardEffects/Effects`: a `[Serializable]` class
   deriving from `BaseCardEffect`. Override `RequiredConfigType` and
   `Execute`.
3. In `Execute`, do not change state. Queue an action:

   ```csharp
   context.CombatManager.ActionQueue.Enqueue(
       new BlockAction(context.CombatManager.CombatSystem, context.Source,
           context.Targets, config.Amount, true),
       null);
   ```

4. If no existing action fits, add one (next recipe).

The card inspector finds new effect classes by itself.

Use `context.CombatManager.Rng` for anything random.

### A new action and rule

An action is one step in the queue. It calls one rule.

1. Add the rule to `CombatSystem` as a `ResolveSomething` method. This is
   where state changes and where buff hooks are called.
2. Add a class in `Actions` implementing `ICombatAction`. `Execute` calls
   the rule and returns. It must not wait for anything.

### A new buff

1. Add data in `Buff/BuffData`: a ScriptableObject deriving from `BuffData`
   that overrides `CreateInstance`.
2. Add the instance in `Buff/BuffInstances`, deriving from `BuffInstance`.
   Implement the hook interfaces you need from `Combat/Hooks/CombatHooks.cs`
   (`IOnBeforeDealDamage`, `IOnCardPlayed`, and so on).
3. To add a hook that does not exist yet: add the interface, a dispatch
   method on `BuffManager`, and call it from the matching `CombatSystem`
   rule.

Timing to know:

- `OnTurnEnd` runs at the end of the owner's own side's turn. Use it for
  effects that last one turn, like Temp Strength.
- `OnRoundEnd` runs once per round after both sides have acted. Buffs
  flagged `expiresAtEndOfTurn` lose one stack here.
- A timed buff applied during the enemy turn skips that round's countdown,
  so it is still active on the next enemy turn.

Do not apply a buff to a unit from inside one of that unit's own hooks. The
buff list is being walked at that moment. Queue an `ApplyBuffAction`.

### A new enemy intent

1. Add a config deriving from `BaseEnemyIntentConfig` and an intent deriving
   from `BaseEnemyIntent`, under `Unit/EnemyUnits/EnemyIntents`.
2. `ExecuteIntent` queues actions, like a card effect.
3. Use `snapshot.Rng` for any random choice.

Intent objects live inside the `EnemyUnitData` asset and are shared by every
enemy of that type. Never store per-enemy state on them. The targets an
enemy picked are kept on the enemy (`CurrentIntentTargets`).

### Something new to show or animate

For a value that just changes, such as a number on a bar, subscribe to the
model's event in the view. That is how the health bar works.

For something that must play in order with the rest of combat:

1. Add an event class in `Combat/Events` implementing `ICombatEvent`. Give
   it only model objects.
2. Publish it from the action: `queue.Publish(new MyEvent(...))`. If it has
   to come after other queued actions, queue a `PublishEventAction`.
3. Handle it in a view that implements `ICombatEventPresenter`. Do instant
   work inside `Present`. Return an `IEnumerator` to make combat wait for an
   animation, or `null` to let it continue.
4. Register the view with the runner in `CombatPresenter.Awake`
   (`queueRunner.AddPresenter(...)`).

Rules never wait for the event to be handled. With no presenter, nothing
happens and combat carries on.

### A new choice for the player in combat

`CardChoiceDecision` is the worked example. Follow it:

1. Add a decision class in `Combat/Decisions` implementing
   `ICombatDecision`. It lists the options and can validate an answer.
2. Add a command in `Combat/Commands` implementing `ICombatCommand`.
3. In `CombatManager`: store the pending decision, suspend the queue, raise
   `DecisionRequested`, and return it from `PendingDecision`. In `Submit`,
   validate the command, apply it, then resume the queue.
4. In `CombatPresenter.PresentDecision`, show the UI. Its buttons submit the
   command.

The rules must not open the UI or wait for it. They only stop until a
command arrives.

### A new target type

Add it to `CardTargeting.GetTargetOptions`, and to `PicksOneUnit` if the
player chooses a single unit. Decisions, command checks and both drop
receivers pick it up from there.

### A node type with its own screen (rest site, merchant, event)

These are skipped today: `RunSession.EnterNode` completes any node that is
not a combat. To give one real content:

1. In `RunSession`, handle the node in `EnterNode`. Add a `RunState` value
   for it, a decision and commands in `Run/RunDecisions.cs`, and call
   `FinishNode()` when the player is done.
2. In `RunManager.HandleRunStateChanged`, show the panel for the new state.
3. The panel's buttons call `RunManager.Instance.Run.Submit(...)`.

Where node types appear on the map (fixed floors, weights, placement rules)
is in `MapBuilder`.

### A boss

`MapBuilder` never assigns `NodeType.CombatBoss`, so a run cannot be won
yet. `RunSession` already ends the run as won when a boss combat is won; the
map only needs to place one.

### Something random

Never use `UnityEngine.Random` or `new System.Random()` in rules. A seed
must reproduce a run.

- In combat: `combat.Rng` (also `snapshot.Rng` and `context.CombatManager.Rng`).
- In the run: `run.LootRng`, `run.EventRng`, `run.ShopRng`.
- For something tied to one map node: `SeedUtility.DeriveNodeSeed`.

### Something to save as progression

1. Add a method to `ICombatProgressRecorder` or `IRunProgressRecorder`.
2. Call it from the rules. `RunSession` does this through `Report(...)`, so
   a recorder that throws cannot break the run.
3. Implement it in `GlobalProgressionRecorder`, forwarding to
   `GlobalProgressionManager`.

## Tests

Tests are in `Assets/Editor/Tests` and run in EditMode, with no scene and no
play mode. Run them from **Window > General > Test Runner**, or in batch
mode with the command in the README.

- `CombatTestRig` builds a seeded combat. Add a player, add enemies, start,
  then submit commands and assert on the result.
- `RunTestRig` builds a node library, a character and reward pools, and can
  play a combat or a whole run for you.

Add a test for every new rule. They are quick to write because nothing
needs a scene:

```csharp
[Test]
public void StrikeDealsItsDamage()
{
    rig.AddPlayer(new[] { strike, strike, strike });
    BaseEnemyUnitInstance enemy = rig.AddEnemy("Enemy", 20);
    rig.StartCombat();

    CardInstance card = rig.Player.Hand.Cards[0];
    rig.Combat.Submit(new PlayCardCommand(card, new[] { enemy }));

    Assert.That(enemy.CurrentHealth, Is.EqualTo(14));
}
```

`CombatSceneWiringTests` loads `CombatNodeScene` and checks that its
components resolve to scripts and still reference each other. Add to it when
you add a serialized reference the game cannot run without.

Tests cover the rules. They do not cover how the game looks or feels, so
play the game after changing a view.

## Scenes and prefabs

- A component is tied to its script by the GUID in the script's `.meta`
  file. Rename or move scripts inside the Unity editor so the `.meta`
  travels with them.
- When you rename a serialized field, add
  `[FormerlySerializedAs("oldName")]` or the scene loses the value.
- When you move rules out of a MonoBehaviour, keep a MonoBehaviour on the
  original `.meta` for the scene side and put the rules in a new plain
  class. `CombatPresenter` and `CombatManager` were split this way.

Two components are left over from that split and do nothing. Remove them in
the editor, then delete their scripts:

- `CombatSystemSceneStub`, in `CombatNodeScene`.
- `BuffManagerPrefabStub`, on the Player and Enemy prefabs.

## Known gaps

- **Nothing enforces the split.** All code compiles into one assembly, so
  the compiler will not stop a rule from referencing a view. Assembly
  definitions would.
- **Progression is not split.** `GlobalProgressionManager` and the unlock
  evaluators are MonoBehaviours that hold rules and state. Rules reach them
  only through the recorder interfaces.
- **A few view details sit on model objects:** the sprite on unit and card
  instances, the screen position on `MapNodeData`, and the highlight event
  on units.
- **Unused files:** `Actions/SpendEnergyAction.cs`, `DiscardPile.cs`,
  `GameManager.cs`, `Events/DiscardEvent.cs`, `Events/DrawEvent.cs`.
- **Views are paced in lockstep.** With the runner attached, each action
  waits for its animations before the next one runs. Views can therefore
  read live model state safely.
