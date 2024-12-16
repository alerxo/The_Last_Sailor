using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
[Serializable, GeneratePropertyBag]
[NodeDescription(name: "BoatRotatesTowardsEnemy", story: "[Agent] rotates towards the closest enemy", category: "Action", id: "42682c91db34407ddb4ac382c20646f1")]
public partial class BoatRotatesTowardsEnemyAction : Action
{
    [SerializeReference] public BlackboardVariable<AIBoatController> Agent;

    protected override Status OnStart()
    {
        if (Agent.Value.Admiral == null || Agent.Value.Admiral.Enemy == null)
        {
            return Status.Failure;
        }

        Boat target = TryGetTarget();

        if (target == null)
        {
            return Status.Failure;
        }

        RotateTowardsTarget(target.transform.position);

        return Status.Success;
    }

    private Boat TryGetTarget()
    {
        float closest = float.MaxValue;
        Boat target = null;

        foreach (Boat boat in Agent.Value.Admiral.Enemy.Fleet)
        {
            float distance = Vector3.Distance(Agent.Value.transform.position, boat.transform.position);

            if (distance < closest && IsValidTarget(boat))
            {
                target = boat;
                closest = distance;
            }
        }

        return target;
    }

    private bool IsValidTarget(Boat _boat)
    {
        return _boat != null && !_boat.IsSunk;
    }

    private void RotateTowardsTarget(Vector3 _position)
    {
        float cross = Vector3.Cross((Agent.Value.transform.position - _position).normalized, Agent.Value.transform.forward).y;
        float rotation = Vector3.Cross((Agent.Value.transform.position - _position).normalized, cross > 0 ? Agent.Value.transform.right : -Agent.Value.transform.right).y;

        Agent.Value.Boat.Engine.ChangeRudderTowards(rotation);
    }
}