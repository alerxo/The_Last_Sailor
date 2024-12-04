using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "BoatDisperseCommand", story: "[Agent] disperses", category: "Action", id: "b0070ae74b0df0810fc4acbda6eae196")]
public partial class BoatDisperseCommandAction : Action
{
    [SerializeReference] public BlackboardVariable<AIBoatController> Agent;

    private const float ENGAGEMENT_RANGE = 50f;

    protected override Status OnStart()
    {
        if (Agent.Value.Admiral.Enemy == null)
        {
            return Status.Failure;
        }

        Transform target = GetTarget();

        if (target == null)
        {
            return Status.Failure;
        }

        MoveTowards(target);

        return Status.Running;
    }

    private Transform GetTarget()
    {
        Transform closest = null;
        float closestDistance = float.MaxValue;

        foreach (Boat boat in Agent.Value.Admiral.Enemy.Fleet)
        {
            float distance;

            if (IsValidTarget(boat) && (distance = Vector3.Distance(Agent.Value.transform.position, boat.transform.position)) < closestDistance)
            {
                closest = boat.transform;
                closestDistance = distance;
            }
        }

        return closest;
    }

    public bool IsValidTarget(Boat _boat)
    {
        return _boat.Health > 0;
    }

    private void MoveTowards(Transform _target)
    {
        if (ShouldMoveRight(_target))
        {
            Agent.Value.SetDestination(_target.position + (_target.right * ENGAGEMENT_RANGE));
        }

        else
        {
            Agent.Value.SetDestination(_target.position - (_target.right * ENGAGEMENT_RANGE));
        }
    }

    private bool ShouldMoveRight(Transform _target)
    {
        return Vector3.Distance(_target.position + (_target.right * ENGAGEMENT_RANGE), Agent.Value.transform.position) < Vector3.Distance(_target.position - (_target.right * ENGAGEMENT_RANGE), Agent.Value.transform.position);
    }
}

