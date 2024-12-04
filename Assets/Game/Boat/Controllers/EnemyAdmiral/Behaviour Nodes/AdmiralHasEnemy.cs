using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AdmiralHasEnemy", story: "Does [Agent] have an enemy. Invert [IsInverted]", category: "Action", id: "6f3fca4f2058ebd3f3a4687c48e318df")]
public partial class HasEnemyAction : Action
{
    [SerializeReference] public BlackboardVariable<EnemyAdmiralController> Agent;
    [SerializeReference] public BlackboardVariable<bool> IsInverted;
    protected override Status OnStart()
    {
        if (IsInverted)
        {
            return !HasEnemy() ? Status.Success : Status.Failure;
        }

        return HasEnemy() ? Status.Success : Status.Failure;
    }

    private bool HasEnemy()
    {
        return Agent.Value.Enemy != null;
    }
}

