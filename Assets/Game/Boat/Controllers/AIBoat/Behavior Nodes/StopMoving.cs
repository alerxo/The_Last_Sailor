using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Stop Moving", story: "[Agent] stops moving", category: "Action", id: "f58e9fd1af841b9fe65f72ea3526ef8d")]
public partial class StopMovingAction : Action
{
    [SerializeReference] public BlackboardVariable<AIBoatController> Agent;
    protected override Status OnStart()
    {
        Agent.Value.SetDistance(0);
        Agent.Value.SetCross(Vector3.zero);
        Agent.Value.Boat.Engine.ChangeThrottle(0);

        return Status.Success;
    }
}