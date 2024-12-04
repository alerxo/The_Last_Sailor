using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "BoatMovesTowardsDestination", story: "[Agent] moves towards destination", category: "Action", id: "afc8648216910974edd7c9623d54b904")]
public partial class BoatMovesTowardsDestination : Action
{
    [SerializeReference] public BlackboardVariable<AIBoatController> Agent;
    public const float APROACH_DISTANCE = 100f;
    private const float ARRIVAL_RANGE = 10f;
    private const float STOP_DISTANCE = 5f;

    protected override Status OnStart()
    {
        if (!Agent.Value.Destination.HasValue || HasArrived())
        {
            return Status.Failure;
        }

        SetDistance();
        SetEngine();

        return Status.Success;
    }

    private void SetEngine()
    {
        float throttle = Mathf.Clamp01((Agent.Value.ForwardCollisionDistance - STOP_DISTANCE) / (APROACH_DISTANCE - STOP_DISTANCE));
        throttle = Mathf.Pow(throttle, 3);
        Agent.Value.Boat.Engine.ChangeTowardsThrottle(throttle * Agent.Value.Speed);

        Vector3 cross = Vector3.Cross((Agent.Value.transform.position - Agent.Value.Destination.Value).normalized, Agent.Value.transform.forward);
        Agent.Value.Boat.Engine.ChangeTowardsRudder(cross.y);
    }

    private void SetDistance()
    {
        Agent.Value.SetDistance(Vector2.Distance(new Vector2(Agent.Value.transform.position.x, Agent.Value.transform.position.z), new Vector2(Agent.Value.Destination.Value.x, Agent.Value.Destination.Value.z)));
    }

    private bool HasArrived()
    {
        return Agent.Value.Distance <= ARRIVAL_RANGE;
    }
}