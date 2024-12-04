using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AdmiralCanEnterCombat", story: "Can [Agent] enter combat. Invert [IsInverted]", category: "Action", id: "55e6ee7b3795d466755aab3e9934b3be")]
public partial class CanEnterCombatAction : Action
{
    [SerializeReference] public BlackboardVariable<EnemyAdmiralController> Agent;
    [SerializeReference] public BlackboardVariable<bool> IsInverted;
    protected override Status OnStart()
    {
        if (IsInverted)
        {
            return !CanEnterCombat() ? Status.Success : Status.Failure;
        }

        return CanEnterCombat() ? Status.Success : Status.Failure;
    }

    private bool CanEnterCombat()
    {
        return CombatManager.Instance.CanEnterRingOfFire();
    }
}

