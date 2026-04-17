using UnityEngine;

[CreateAssetMenu(fileName = "new EventNodeConfig", menuName = "Nodes/EventNodeConfig")]
public class EventNodeConfig : BaseNodeConfig
{
    // TODO: event config
    public override NodeType NodeType => NodeType.Event;
    public override BaseNode CreateNode() => new EventNode(this);
}
