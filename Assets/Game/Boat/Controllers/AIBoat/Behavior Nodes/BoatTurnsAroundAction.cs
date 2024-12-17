using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "BoatTurnsAround", story: "[Agent] turns around", category: "Action", id: "10fd80f65cb28fcd8cacffa77f030252")]
public partial class BoatTurnsAroundAction : Action
{
    [SerializeReference] public BlackboardVariable<AIBoatController> Agent;

    protected override Status OnStart()
    {
        if (!Agent.Value.HasTrail())
        {
            return Status.Failure;
        }

        SetRudder();
        SetThrottle();

        return Status.Success;
    }

    private void SetRudder()
    {
        float cross = Vector3.Cross((Agent.Value.transform.position - Agent.Value.GetCurrentTrail()).normalized, Agent.Value.transform.forward).y;
        cross = cross > 0 ? 1 : -1;
        Agent.Value.Boat.Engine.ChangeRudderTowards(cross, 10);
    }

    private void SetThrottle()
    {
        Agent.Value.Boat.Engine.ChangeThrottleTowards(0);
    }
}