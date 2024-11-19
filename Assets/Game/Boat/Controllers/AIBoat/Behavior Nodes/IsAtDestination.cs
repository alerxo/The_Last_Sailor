using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Is At Destination", story: "[Agent] is at destination [IsInverted] Invert", category: "Action", id: "4d53ff69fc86e9981d77278829d3e609")]
public partial class IsAtDestinationAction : Action
{
    [SerializeReference] public BlackboardVariable<AIBoatController> Agent;
    [SerializeReference] public BlackboardVariable<bool> IsInverted;
    private const float ARRIVAL_RANGE = 10f;

    protected override Status OnStart()
    {
        Agent.Value.distance = Vector2.Distance(new Vector2(Agent.Value.transform.position.x, Agent.Value.transform.position.z), new Vector2(Agent.Value.Destination.Value.x, Agent.Value.Destination.Value.z));

        if (IsInverted)
        {
            return !IsAtDestination() ? Status.Success : Status.Failure;
        }

        return  IsAtDestination() ? Status.Success : Status.Failure;
    }

    private bool IsAtDestination()
    {
        return Agent.Value.distance <= ARRIVAL_RANGE;
    }
}