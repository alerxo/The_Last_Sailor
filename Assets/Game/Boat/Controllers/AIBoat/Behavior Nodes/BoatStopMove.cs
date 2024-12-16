using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "BoatStopMove", story: "[Agent] stops moving", category: "Action", id: "f58e9fd1af841b9fe65f72ea3526ef8d")]
public partial class StopMovingAction : Action
{
    [SerializeReference] public BlackboardVariable<AIBoatController> Agent;

    protected override Status OnStart()
    {
        Agent.Value.Boat.Engine.ChangeThrottleTowards(0);
        Agent.Value.Boat.Engine.ChangeRudderTowards(0);

        return Status.Success;
    }
}