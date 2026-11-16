using System;

public class UnknownNode : BaseNode
{
    private bool _resolved;
    private UnknownOutcomeType _resolvedOutcome;
    private BaseNode _resolvedInnerNode;

    public override BaseNode ResolvedNode => _resolvedInnerNode ?? this;

    private NodeConfigLibrary _library;
    private int _runSeed;

    public UnknownNode(UnknownNodeConfig config) : base(config)
    {
    }

    /// <summary>
    /// Gives the node what it needs to decide, when it is entered, what it
    /// turns into. The outcome depends only on the run seed and the node's
    /// place on the map.
    /// </summary>
    public void Configure(NodeConfigLibrary library, int runSeed)
    {
        _library = library;
        _runSeed = runSeed;
    }

    public override void OnEnter()
    {
        if (!_resolved)
            ResolveUnknown();

        _resolvedInnerNode.OnEnter();
    }

    public override void OnExit()
    {
        _resolvedInnerNode?.OnExit();
    }

    private void ResolveUnknown()
    {
        if (_library == null)
        {
            throw new InvalidOperationException(
                "UnknownNode was entered before Configure gave it a node config library.");
        }

        int outcomeSeed = SeedUtility.DeriveNodeSeed(
            _runSeed,
            Act,
            Floor,
            Index,
            RngCategory.UnknownOutcome
        );

        int contentSeed = SeedUtility.DeriveNodeSeed(
            _runSeed,
            Act,
            Floor,
            Index,
            RngCategory.UnknownContent
        );

        System.Random outcomeRng = new System.Random(outcomeSeed);
        System.Random contentRng = new System.Random(contentSeed);

        _resolvedOutcome = RollOutcome(outcomeRng);

        NodeConfigLibrary library = _library;

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