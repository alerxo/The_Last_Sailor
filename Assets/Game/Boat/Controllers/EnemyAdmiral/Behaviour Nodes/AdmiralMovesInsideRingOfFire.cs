using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AdmiralMovesInsideRingOfFire", story: "[Agent] moves inside ring of fire", category: "Action", id: "ac01035859be8b85dccf603000b2c48d")]
public partial class SetDestinationInsideRingOfFireAction : Action
{
    [SerializeReference] public BlackboardVariable<EnemyAdmiralController> Agent;
    protected override Status OnStart()
    {
        Agent.Value.SetDestination(CombatManager.GetPositionInSideRingOfFire());

        return Status.Success;
    }
}