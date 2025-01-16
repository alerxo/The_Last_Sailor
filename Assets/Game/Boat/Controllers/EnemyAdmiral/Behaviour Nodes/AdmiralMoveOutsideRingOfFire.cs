using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AdmiralMoveOutsideRingOfFire", story: "[Agent] moves outside ring of fire", category: "Action", id: "139a1e0b08c7b2c01a55cbf208ef0339")]
public partial class AdmiralMoveOutsideRingOfFire : Action
{
    [SerializeReference] public BlackboardVariable<EnemyAdmiralController> Agent;
    protected override Status OnStart()
    {
        Agent.Value.SetDestination(CombatManager.Instance.GetPositionOutSideRingOfFire());

        return Status.Success;
    }
}