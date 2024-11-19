using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SetDestinationOutsideRingOfFire", story: "[Agent] sets its destination outside ring of fire", category: "Action", id: "139a1e0b08c7b2c01a55cbf208ef0339")]
public partial class SetDestinationOutsideRingOfFire : Action
{
    [SerializeReference] public BlackboardVariable<EnemyAdmiralController> Agent;
    protected override Status OnStart()
    {
        Agent.Value.BoatController.SetDestination(CombatManager.GetClosestPositionOutSideRingOfFire(Agent.Value.transform.position));

        return Status.Success;
    }
}