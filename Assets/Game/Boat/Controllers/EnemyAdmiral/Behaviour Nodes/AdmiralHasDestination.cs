using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AdmiralHasDestination", story: "[Agent] has a destination [IsInverted] Invert", category: "Action", id: "28cf39241447f75d41794f18b69b5ce0")]
public partial class AdmiralHasDestinationAction : Action
{
    [SerializeReference] public BlackboardVariable<EnemyAdmiralController> Agent;
    [SerializeReference] public BlackboardVariable<bool> IsInverted;
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
        return Agent.Value.BoatController.Destination != null;
    }
}