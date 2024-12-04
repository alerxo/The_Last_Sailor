using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "BoatFormationCommand", story: "[Agent] goes to formation", category: "Action", id: "652ea056728fd7c3a3468c95fe89023b")]
public partial class BoatFormationCommandAction : Action
{
    [SerializeReference] public BlackboardVariable<AIBoatController> Agent;

    protected override Status OnStart()
    {
        if (Agent.Value.Admiral == null || !Agent.Value.FormationPosition.HasValue)
        {
            return Status.Failure;
        }

        Agent.Value.SetDestination(Agent.Value.GetFormationPositionInWorld());

        return Status.Running;
    }
}

