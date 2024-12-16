using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "BoatHoldCommand", story: "[Agent] holds at position", category: "Action", id: "b0070ae74b0df0810fc4acbda6eae196")]
public partial class BoatDisperseCommandAction : Action
{
    [SerializeReference] public BlackboardVariable<AIBoatController> Agent;

    protected override Status OnStart()
    {
        if (Agent.Value.Admiral == null || !Agent.Value.HoldPosition.HasValue)
        {
            return Status.Failure;
        }

        Agent.Value.SetDestination(Agent.Value.HoldPosition.Value);

        return Status.Running;
    }
}