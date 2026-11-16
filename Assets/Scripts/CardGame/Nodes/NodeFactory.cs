using System;
using UnityEngine;

public static class NodeFactory
{
    public static BaseNode CreateNode(MapNodeData mapNode, System.Random rng, NodeConfigLibrary library, int runSeed)
    {
        BaseNode node = mapNode.AssignedType switch
        {
            NodeType.CombatBoss => new BossCombatNode(GetRandomConfig(library.BossCombatNodeConfigs, rng)),
            NodeType.CombatElite => new EliteCombatNode(GetRandomConfig(library.EliteCombatNodeConfigs, rng)),
            NodeType.CombatNonElite => new NonEliteCombatNode(GetRandomConfig(library.nonEliteCombatNodeConfigs, rng)),
            NodeType.Event => new EventNode(GetRandomConfig(library.EventNodeConfigs, rng)),
            NodeType.Merchant => new MerchantNode(GetRandomConfig(library.MerchantNodeConfigs, rng)),
            NodeType.RestSite => new RestSiteNode(GetRandomConfig(library.RestSiteNodeConfigs, rng)),
            NodeType.Treasure => new TreasureNode(GetRandomConfig(library.TreasureNodeConfigs, rng)),
            NodeType.Unknown => new UnknownNode(library.UnknownNodeConfig),
            _ => throw new ArgumentOutOfRangeException(nameof(mapNode.AssignedType), mapNode.AssignedType, null)
        };
        node.SetNodeData(mapNode);

        if (node is UnknownNode unknownNode)
        {
            unknownNode.Configure(library, runSeed);
        }

        return node;
    }
    
    
    private static T GetRandomConfig<T>(System.Collections.Generic.List<T> configs, System.Random rng)
    {
        if (configs == null || configs.Count == 0)
            throw new InvalidOperationException($"No configs found for {typeof(T).Name}");

        int randomIndex = RngUtility.Range(rng, 0, configs.Count);
        return configs[randomIndex];
    }
}
