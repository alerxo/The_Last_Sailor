using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MoveTowardsEnemy", story: "[Agent] moves towards enemy", category: "Action", id: "f6e4d0f7cf7aae0dea241a65a581d6d3")]
public partial class MoveTowardsEnemyAction : Action
{
    [SerializeReference] public BlackboardVariable<EnemyAdmiralController> Agent;
    private const float ENGAGEMENT_RANGE = 150f;

    protected override Status OnStart()
    {
        if (Vector3.Distance(Agent.Value.Enemy.transform.position + (Agent.Value.Enemy.transform.right * ENGAGEMENT_RANGE), Agent.Value.transform.position) <
            Vector3.Distance(Agent.Value.Enemy.transform.position - (Agent.Value.Enemy.transform.right * ENGAGEMENT_RANGE), Agent.Value.transform.position))
        {
            Agent.Value.SetDestination(Agent.Value.Enemy.transform.position + (Agent.Value.Enemy.transform.right * ENGAGEMENT_RANGE));
        }

        else
        {
            Agent.Value.SetDestination(Agent.Value.Enemy.transform.position - (Agent.Value.Enemy.transform.right * ENGAGEMENT_RANGE));
        }

        return Status.Success;
    }
}