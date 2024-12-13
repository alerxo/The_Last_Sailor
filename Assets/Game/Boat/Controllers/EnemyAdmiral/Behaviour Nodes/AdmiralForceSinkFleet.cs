using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AdmiralForceSinkFleet", story: "[Agent] fleet is forced to sink", category: "Action", id: "6e176f122df4d547d7c0950895ee7b2e")]
public partial class ForceSinkAction : Action
{
    [SerializeReference] public BlackboardVariable<EnemyAdmiralController> Agent;
    protected override Status OnStart()
    {
        CombatManager.Instance.ForceSinkEnemy();

        return Status.Running;
    }
}

