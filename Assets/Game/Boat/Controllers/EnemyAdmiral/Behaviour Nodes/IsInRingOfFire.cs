using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "IsInRingOfFire", story: "[Agent] is in ring of fire [IsInverted] Invert", category: "Action", id: "0580de8e12e2e973c0db4a76574826bf")]
public partial class IsInRingOfFireAction : Action
{
    [SerializeReference] public BlackboardVariable<EnemyAdmiralController> Agent;
    [SerializeReference] public BlackboardVariable<bool> IsInverted;
    protected override Status OnStart()
    {
        if (IsInverted)
        {
            return !IsInRingOfFire() ? Status.Success : Status.Failure;
        }

        return IsInRingOfFire() ? Status.Success : Status.Failure;
    }

    private bool IsInRingOfFire()
    {
        return Vector3.Distance(PlayerBoatController.Instance.transform.position, Agent.Value.transform.position) < CombatManager.RING_OF_FIRE;
    }
}

