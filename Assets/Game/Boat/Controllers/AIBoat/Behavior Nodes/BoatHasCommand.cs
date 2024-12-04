using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "BoatHasCommand", story: "Does [Agent] have the command [Command]", category: "Action", id: "e14d596162d153a581e9aa02ae88a70d")]
public partial class BoatHasCommandAction : Action
{
    [SerializeReference] public BlackboardVariable<AIBoatController> Agent;
    [SerializeReference] public BlackboardVariable<int> Command;
    protected override Status OnStart()
    {
        return (int)Agent.Value.Command == Command ? Status.Success : Status.Failure;
    }
}

