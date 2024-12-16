using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "BoatUnassignedCommand", story: "[Agent] runs unassigned command", category: "Action", id: "e51cd452556adf6ec57d320a0bda6400")]
public partial class BoatUnassignedCommandAction : Action
{
    [SerializeReference] public BlackboardVariable<AIBoatController> Agent;
    protected override Status OnStart()
    {
        return Status.Running;
    }
}