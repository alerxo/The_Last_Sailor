using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AdmiralHasDestination", story: "Does [Agent] have a destination. Invert [IsInverted]", category: "Action", id: "28cf39241447f75d41794f18b69b5ce0")]
public partial class AdmiralHasDestinationAction : Action
{
    [SerializeReference] public BlackboardVariable<EnemyAdmiralController> Agent;
    [SerializeReference] public BlackboardVariable<bool> IsInverted;

    private const int CLEAR_TRAIL_DISTANCE = 100;

    protected override Status OnStart()
    {
        if (IsInverted)
        {
            return !HasDestination() ? Status.Success : Status.Failure;
        }

        return HasDestination() ? Status.Success : Status.Failure;
    }

    private bool HasDestination()
    {
        if (Agent.Value.AIBoatController.Distance < CLEAR_TRAIL_DISTANCE)
        {
            Agent.Value.AIBoatController.ClearTrail();
        }

        return Agent.Value.AIBoatController.HasDestination();
    }
}