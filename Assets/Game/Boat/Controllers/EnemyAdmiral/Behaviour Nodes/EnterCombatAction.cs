using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "EnterCombat", story: "[Agent] enters combat [IsInverted] Invert", category: "Action", id: "e4b7907e48070e558d1518853cc60a31")]
public partial class EnterCombatAction : Action
{
    [SerializeReference] public BlackboardVariable<EnemyAdmiralController> Agent;
    [SerializeReference] public BlackboardVariable<bool> IsInverted;

    protected override Status OnStart()
    {
        if (IsInverted)
        {
            Agent.Value.SetEnemy(null);
            CombatManager.Instance.ExitCombat(Agent.Value);

            return Status.Success;
        }

        Agent.Value.SetEnemy(PlayerBoatController.Instance.AdmiralController);
        CombatManager.Instance.EnterCombat(Agent.Value);

        return Status.Success;
    }
}