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

    public const float APROACH_DISTANCE = 100f;
    private const float STOP_DISTANCE = 5f;

    protected override Status OnStart()
    {
        if (Agent.Value.Destination == null)
        {
            return Status.Failure;
        }

        float throttle = Mathf.Clamp01((Agent.Value.ForwardCollisionDistance - STOP_DISTANCE) / (APROACH_DISTANCE - STOP_DISTANCE));
        throttle = Mathf.Pow(throttle, 3);
        Agent.Value.Boat.Engine.ChangeTowardsThrottle(throttle * Agent.Value.Speed);

        Agent.Value.SetCross(Vector3.Cross((Agent.Value.transform.position - Agent.Value.Destination.Value).normalized, Agent.Value.transform.forward));
        Agent.Value.Boat.Engine.ChangeTowardsRudder(Agent.Value.Cross.y);

        return Status.Success;
    }
}