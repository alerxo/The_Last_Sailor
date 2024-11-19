using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Has Destination", story: "[Agent] has a destination [IsInverted] Invert", category: "Action", id: "8ebceffe0177af62224ce934375184b2")]
public partial class HasDestinationAction : Action
{
    [SerializeReference] public BlackboardVariable<AIBoatController> Agent;
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
        return Agent.Value.Destination != null;
    }
}

