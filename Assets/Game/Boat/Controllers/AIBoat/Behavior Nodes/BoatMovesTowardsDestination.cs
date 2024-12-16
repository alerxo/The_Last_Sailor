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
    public const float STOP_DISTANCE = 5f;
    private float distance;

    protected override Status OnStart()
    {
        if (!Agent.Value.HasTrail())
        {
            return Status.Failure;
        }

        SetDistance();

        if (HasArrived())
        {
            Agent.Value.ConsumeTrail();
        }

        if (!Agent.Value.HasTrail())
        {
            return Status.Failure;
        }

        SetEngine();

        return Status.Success;
    }

    private void SetDistance()
    {
        distance = GetDistance(Agent.Value.transform.position, Agent.Value.GetCurrentTrail());
    }

    private float GetDistance(Vector3 _first, Vector3 _second)
    {
        return Vector2.Distance(new Vector2(_first.x, _first.z), new Vector2(_second.x, _second.z));
    }

    private bool HasArrived()
    {
        return (Agent.Value.HasNextTrail() && distance <= AIBoatController.TRAIL_DISTANCE) || distance <= STOP_DISTANCE;
    }

    private void SetEngine()
    {
        if (IsTrailInFrontOfAgent()) MoveForward();
        else TurnAround();
    }

    private bool IsTrailInFrontOfAgent()
    {
        return Vector3.Dot((Agent.Value.transform.position - Agent.Value.GetCurrentTrail()).normalized, Agent.Value.transform.forward) < 0;
    }

    private void TurnAround()
    {
        Agent.Value.Boat.Engine.ChangeThrottleTowards(0);
        float cross = Vector3.Cross((Agent.Value.transform.position - Agent.Value.GetCurrentTrail()).normalized, Agent.Value.transform.forward).y;
        cross = cross > 0 ? 1 : -1;
        Agent.Value.Boat.Engine.ChangeRudderTowards(cross);
    }

    private void MoveForward()
    {
        float throttle = Mathf.Clamp01((Agent.Value.ForwardCollisionDistance - STOP_DISTANCE) / (APROACH_DISTANCE - STOP_DISTANCE));
        throttle = Mathf.Clamp(Mathf.Pow(throttle, 3), 0.1f, 1);
        Agent.Value.Boat.Engine.ChangeThrottleTowards(throttle * Agent.Value.Speed);

        float cross = Vector3.Cross((Agent.Value.transform.position - Agent.Value.GetCurrentTrail()).normalized, Agent.Value.transform.forward).y;

        if (Agent.Value.HasNextTrail())
        {
            cross += Vector3.Cross((Agent.Value.transform.position - Agent.Value.GetNextTrail()).normalized, Agent.Value.transform.forward).y;
            cross /= 2;
        }

        Agent.Value.Boat.Engine.ChangeRudderTowards(cross);
    }
}