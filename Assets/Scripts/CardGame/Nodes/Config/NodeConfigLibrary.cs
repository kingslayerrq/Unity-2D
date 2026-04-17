using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New NodeConfigLibrary", menuName = "Nodes/NodeConfigLibrary")]
public class NodeConfigLibrary : ScriptableObject
{
    public List<NonEliteCombatNodeConfig> nonEliteCombatNodeConfigs;
    public List<EliteCombatNodeConfig> EliteCombatNodeConfigs;
    public List<BossCombatNodeConfig> BossCombatNodeConfigs;
    public List<MerchantNodeConfig> MerchantNodeConfigs;
    public List<RestSiteNodeConfig> RestSiteNodeConfigs;
    public List<TreasureNodeConfig> TreasureNodeConfigs;
    public List<EventNodeConfig> EventNodeConfigs;
    public UnknownNodeConfig UnknownNodeConfig;
}
