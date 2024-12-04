using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "BoatIsAlive", story: "Is [Agent] alive", category: "Action", id: "94c68b664c9e83542b42ae3fc4d0c482")]
public partial class IsDestroyedAction : Action
{
    [SerializeReference] public BlackboardVariable<AIBoatController> Agent;
    protected override Status OnStart()
    {
        return IsAlive() ? Status.Success : Status.Failure;
    }

    private bool IsAlive()
    {
        return Agent.Value.Boat.Health > 0;
    }
}

