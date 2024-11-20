using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Move Towards Destination", story: "[Agent] moves towards destination", category: "Action", id: "afc8648216910974edd7c9623d54b904")]
public partial class MoveTowardsDestination : Action
{
    [SerializeReference] public BlackboardVariable<AIBoatController> Agent;

    private const float APROACH_DISTANCE = 20f;

    protected override Status OnStart()
    {
        if (Agent.Value.Destination == null)
        {
            return Status.Failure;
        }

        Agent.Value.Boat.Engine.ChangeThrottle(Agent.Value.Distance < APROACH_DISTANCE ? Agent.Value.Distance / (APROACH_DISTANCE * 0.25f) : 1);
        Agent.Value.SetCross(Vector3.Cross((Agent.Value.transform.position - Agent.Value.Destination.Value).normalized, Agent.Value.transform.forward));
        Agent.Value.Boat.Engine.ChangeRudder(Agent.Value.Cross.y);

        return Status.Success;
    }
}