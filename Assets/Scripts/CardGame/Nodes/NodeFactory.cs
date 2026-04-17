using System;
using UnityEngine;

public static class NodeFactory
{
    public static BaseNode CreateNode(NodeType nodeType,
        int act, int floor, int index, System.Random rng, NodeConfigLibrary library)
    {
        BaseNode node = nodeType switch
        {
            NodeType.CombatBoss => new BossCombatNode(GetRandomConfig(library.BossCombatNodeConfigs, rng)),
            NodeType.CombatElite => new EliteCombatNode(GetRandomConfig(library.EliteCombatNodeConfigs, rng)),
            NodeType.CombatNonElite => new NonEliteCombatNode(GetRandomConfig(library.nonEliteCombatNodeConfigs, rng)),
            NodeType.Event => new EventNode(GetRandomConfig(library.EventNodeConfigs, rng)),
            NodeType.Merchant => new MerchantNode(GetRandomConfig(library.MerchantNodeConfigs, rng)),
            NodeType.RestSite => new RestSiteNode(GetRandomConfig(library.RestSiteNodeConfigs, rng)),
            NodeType.Treasure => new TreasureNode(GetRandomConfig(library.TreasureNodeConfigs, rng)),
            NodeType.Unknown => new UnknownNode(library.UnknownNodeConfig),
            _ => throw new ArgumentOutOfRangeException(nameof(nodeType), nodeType, null)
        };
        node.Act = act;
        node.Floor = floor;
        node.Index = index;
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
