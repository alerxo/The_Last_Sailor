using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AdmiralIsInCombat", story: "Is [Agent] in combat. Invert [IsInverted]", category: "Action", id: "53f6e7743204faefcbe64c396644bb97")]
public partial class IsInCombatAction : Action
{
    [SerializeReference] public BlackboardVariable<EnemyAdmiralController> Agent;
    [SerializeReference] public BlackboardVariable<bool> IsInverted;
    protected override Status OnStart()
    {
        if (IsInverted)
        {
            return !IsInCombat() ? Status.Success : Status.Failure;
        }

        return IsInCombat() ? Status.Success : Status.Failure;
    }

    private bool IsInCombat()
    {
        return Agent.Value == CombatManager.Instance.AdmiralInRingOfFireBuffer;
    }
}

