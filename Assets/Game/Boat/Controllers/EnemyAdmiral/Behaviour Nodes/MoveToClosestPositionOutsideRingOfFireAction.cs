using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MoveToClosestPositionOutsideRingOfFire", story: "[Agent] moves towards the closest position outside the ring of fire", category: "Action", id: "4bfff8ea3bce3f0964c6a6c8d802ae7a")]
public partial class MoveToClosestPositionOutsideRingOfFireAction : Action
{
    [SerializeReference] public BlackboardVariable<EnemyAdmiralController> Agent;

    protected override Status OnStart()
    {
        Agent.Value.SetDestination(CombatManager.GetClosestPositionOutSideRingOfFire(Agent.Value.transform.position));

        return Status.Success;
    }
}