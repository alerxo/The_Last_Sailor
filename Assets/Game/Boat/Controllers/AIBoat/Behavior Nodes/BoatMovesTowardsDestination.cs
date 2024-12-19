using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using Unity.VisualScripting;
using Unity.AppUI.UI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "BoatMovesTowardsDestination", story: "[Agent] moves towards destination", category: "Action", id: "afc8648216910974edd7c9623d54b904")]
public partial class BoatMovesTowardsDestination : Action
{
    [SerializeReference] public BlackboardVariable<AIBoatController> Agent;
    public const float APROACH_DISTANCE = 50f;
    public const float STOP_DISTANCE = 5f;
    private float trailDistance;
    private RaycastHit obstacle;

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
            return Status.Success;
        }

        CheckForwardCollision();
        SetRudder();
        SetThrottle();

        return Status.Success;
    }

    private void SetDistance()
    {
        trailDistance = GetDistance(Agent.Value.transform.position, Agent.Value.GetCurrentTrail());
    }

    private float GetDistance(Vector3 _first, Vector3 _second)
    {
        return Vector2.Distance(new Vector2(_first.x, _first.z), new Vector2(_second.x, _second.z));
    }

    private bool HasArrived()
    {
        return trailDistance <= STOP_DISTANCE;
    }

    private void CheckForwardCollision()
    {
        RaycastHit closestHit = new()
        {
            distance = APROACH_DISTANCE,
            point = Agent.Value.transform.position
        };

        foreach (Transform ray in Agent.Value.CollisionRays)
        {
            if (Physics.Raycast(ray.position, ray.forward, out RaycastHit hit, APROACH_DISTANCE, ~0, QueryTriggerInteraction.Ignore))
            {
                if (hit.distance < closestHit.distance)
                {
                    closestHit = hit;
                }
            }
        }

        if (closestHit.distance < APROACH_DISTANCE)
        {
            float angle = Vector3.Angle(Agent.Value.transform.forward, (closestHit.point - Agent.Value.transform.position).normalized);
            float brake = Mathf.Lerp(1f, 4f, Mathf.Clamp01(angle / 90f));
            closestHit.distance *= brake;

#if UNITY_EDITOR
            if (Agent.Value.IsDebugMode)
            {
                Debug.DrawLine(Agent.Value.transform.position, closestHit.point, Color.Lerp(Color.red, Color.green, closestHit.distance / APROACH_DISTANCE), 0.1f);
            }
#endif
        }

        obstacle = closestHit;
    }

    private void SetRudder()
    {
        float cross = Vector3.Cross((Agent.Value.transform.position - Agent.Value.GetCurrentTrail()).normalized, Agent.Value.transform.forward).y;

        if (Agent.Value.HasNextTrail())
        {
            cross += Vector3.Cross((Agent.Value.transform.position - Agent.Value.GetNextTrail()).normalized, Agent.Value.transform.forward).y;
            cross /= 2;
        }

        if (obstacle.distance < APROACH_DISTANCE)
        {
            float obstacleCross = -Vector3.Cross((Agent.Value.transform.position - obstacle.point).normalized, Agent.Value.transform.forward).y;
            obstacleCross = obstacleCross > 0 ? 1 : -1;
            cross += obstacleCross;
            cross /= 2;
        }

        Agent.Value.Boat.Engine.ChangeRudderTowards(cross, 10);
    }

    private void SetThrottle()
    {
        float approach = Mathf.Clamp01((Mathf.Min(obstacle.distance, trailDistance) - STOP_DISTANCE) / (APROACH_DISTANCE - STOP_DISTANCE));
        approach = Mathf.Clamp(Mathf.Pow(approach, 3), 0.1f, 1);
        float rudder = Mathf.Lerp(1f, 0.5f, Mathf.Abs(Mathf.Clamp01(Agent.Value.Boat.Engine.Rudder - 0.5f)));
        Agent.Value.Boat.Engine.ChangeThrottleTowards(approach * Agent.Value.Speed * rudder, 10);
    }
}