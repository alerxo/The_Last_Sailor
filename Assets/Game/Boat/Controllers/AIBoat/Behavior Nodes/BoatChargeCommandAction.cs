using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "BoatChargeCommand", story: "[Boat] charges", category: "Action", id: "0c9af263cfa955f25da88aaa83cefb44")]
public partial class BoatChargeCommandAction : Action
{
    [SerializeReference] public BlackboardVariable<AIBoatController> Boat;

    protected override Status OnStart()
    {
        return Status.Failure;
    }
}

