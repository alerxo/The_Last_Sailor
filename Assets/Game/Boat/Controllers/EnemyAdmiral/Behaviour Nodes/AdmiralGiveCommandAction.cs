using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AdmiralGiveCommand", story: "[Admiral] gives command [Command]", category: "Action", id: "125e2732f956795d6e446919216e280b")]
public partial class AdmiralGiveCommandAction : Action
{
    [SerializeReference] public BlackboardVariable<EnemyAdmiralController> Admiral;
    [SerializeReference] public BlackboardVariable<int> Command;

    protected override Status OnStart()
    {
        Admiral.Value.SetCommandForSubordinates(Command);

        return Status.Running;
    }
}