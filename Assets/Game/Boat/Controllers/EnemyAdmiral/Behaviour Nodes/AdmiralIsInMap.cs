using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AdmiralIsInMap", story: "Is [Agent] in map. Invert [IsInverted]", category: "Action", id: "ea0391b5ac16799b4b39255bce26c0be")]
public partial class IsInsideMapAction : Action
{
    [SerializeReference] public BlackboardVariable<EnemyAdmiralController> Agent;
    [SerializeReference] public BlackboardVariable<bool> IsInverted;
    protected override Status OnStart()
    {
        if (IsInverted)
        {
            return !IsInsideMap() ? Status.Success : Status.Failure;
        }

        return IsInsideMap() ? Status.Success : Status.Failure;
    }

    private bool IsInsideMap()
    {
        return Vector3.Distance(PlayerBoatController.Instance.transform.position, Agent.Value.transform.position) < CombatManager.GetMapSize();
    }
}