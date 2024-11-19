using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "IsDestroyed", story: "[Agent] is destroyed [IsInverted] Invert", category: "Action", id: "94c68b664c9e83542b42ae3fc4d0c482")]
public partial class IsDestroyedAction : Action
{
    [SerializeReference] public BlackboardVariable<AIBoatController> Agent;
    [SerializeReference] public BlackboardVariable<bool> IsInverted;
    protected override Status OnStart()
    {
        if (IsInverted)
        {
            return !IsDestroyed() ? Status.Success : Status.Failure;
        }

        return IsDestroyed() ? Status.Success : Status.Failure;
    }

    private bool IsDestroyed()
    {
        return Agent.Value.Boat.Health == 0;
    }
}

