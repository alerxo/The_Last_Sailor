using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AdmiralMoveTowardsPlayer", story: "[Agent] moves towards player", category: "Action", id: "6b28d345b93bda6e20dc0c47ff25a23f")]
public partial class SetDestinationAtPlayerLocationAction : Action
{
    [SerializeReference] public BlackboardVariable<EnemyAdmiralController> Agent;
    protected override Status OnStart()
    {
        Agent.Value.SetDestination(PlayerBoatController.Instance.transform.position);

        return Status.Running;
    }
}

