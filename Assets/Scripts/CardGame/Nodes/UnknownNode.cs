using System;

public class UnknownNode : BaseNode
{
    private bool _resolved;
    private UnknownOutcomeType _resolvedOutcome;
    private BaseNode _resolvedInnerNode;

    public UnknownNode(UnknownNodeConfig config) : base(config)
    {
    }

    public override void Enter(RunManager runManager)
    {
        if (!_resolved)
        {
            ResolveUnknown(runManager);
        }

        _resolvedInnerNode.Enter(runManager);
    }

    public override void Exit(RunManager runManager)
    {
        _resolvedInnerNode?.Exit(runManager);
    }

    private void ResolveUnknown(RunManager runManager)
    {
        int outcomeSeed = SeedUtility.DeriveNodeSeed(
            runManager.CurrentRunSeed,
            Act,
            Floor,
            Index,
            RngCategory.UnknownOutcome
        );

        int contentSeed = SeedUtility.DeriveNodeSeed(
            runManager.CurrentRunSeed,
            Act,
            Floor,
            Index,
            RngCategory.UnknownContent
        );

        System.Random outcomeRng = new System.Random(outcomeSeed);
        System.Random contentRng = new System.Random(contentSeed);

        _resolvedOutcome = RollOutcome(outcomeRng);

        NodeConfigLibrary library = runManager.NodeConfigLibrary;

        _resolvedInnerNode = _resolvedOutcome switch
        {
            UnknownOutcomeType.Event => CreateResolvedNode(
                GetRandomConfig(library.EventNodeConfigs, contentRng)),

            UnknownOutcomeType.Combat => CreateResolvedNode(
                GetRandomConfig(library.nonEliteCombatNodeConfigs, contentRng)),

            UnknownOutcomeType.Merchant => CreateResolvedNode(
                GetRandomConfig(library.MerchantNodeConfigs, contentRng)),

            UnknownOutcomeType.Treasure => CreateResolvedNode(
                GetRandomConfig(library.TreasureNodeConfigs, contentRng)),

            _ => throw new ArgumentOutOfRangeException()
        };

        _resolved = true;
    }

    private BaseNode CreateResolvedNode(BaseNodeConfig config)
    {
        BaseNode node = config switch
        {
            NonEliteCombatNodeConfig combatConfig => new NonEliteCombatNode(combatConfig),
            EventNodeConfig eventConfig => new EventNode(eventConfig),
            MerchantNodeConfig merchantConfig => new MerchantNode(merchantConfig),
            TreasureNodeConfig treasureConfig => new TreasureNode(treasureConfig),
            _ => throw new ArgumentOutOfRangeException(nameof(config), config, null)
        };

        node.SetNodeData(NodeData);
        return node;
    }

    private UnknownOutcomeType RollOutcome(System.Random rng)
    {
        int roll = RngUtility.Range(rng, 0, 100);

        if (roll < 50) return UnknownOutcomeType.Event;
        if (roll < 80) return UnknownOutcomeType.Combat;
        if (roll < 90) return UnknownOutcomeType.Treasure;
        return UnknownOutcomeType.Merchant;
    }

    private T GetRandomConfig<T>(System.Collections.Generic.List<T> configs, System.Random rng)
    {
        if (configs == null || configs.Count == 0)
            throw new InvalidOperationException($"No configs found for {typeof(T).Name}");

        int index = RngUtility.Range(rng, 0, configs.Count);
        return configs[index];
    }
}

public enum UnknownOutcomeType
{
    Event,
    Combat,
    Merchant,
    Treasure
}