using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "BoatUnassignedCommand", story: "[Agent] follows admiral", category: "Action", id: "e51cd452556adf6ec57d320a0bda6400")]
public partial class BoatUnassignedCommandAction : Action
{
    [SerializeReference] public BlackboardVariable<AIBoatController> Agent;
    protected override Status OnStart()
    {
        if (Agent.Value.Admiral == null)
        {
            return Status.Failure;
        }

        if (IsAdmiral())
        {
            return Status.Running;
        }

        Agent.Value.SetDestination(Agent.Value.Admiral.transform.position);

        return Status.Running;
    }

    private bool IsAdmiral()
    {
        return Agent.Value.Admiral.Owner == Agent.Value.Boat;
    }
}