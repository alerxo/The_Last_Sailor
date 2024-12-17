using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "BoatDestinationIsInFront", story: "Is [Agent] destination in front", category: "Action", id: "0ed64a3b36b86b4e43ac233ab91f4161")]
public partial class BoatDestinationIsInFrontAction : Action
{
    [SerializeReference] public BlackboardVariable<AIBoatController> Agent;

    protected override Status OnStart()
    {
        if (!Agent.Value.HasTrail())
        {
            return Status.Failure;
        }

        if (Vector3.Dot((Agent.Value.transform.position - Agent.Value.GetCurrentTrail()).normalized, Agent.Value.transform.forward) < 0)
        {
            return Status.Success;
        }

        return Status.Failure;
    }
}