using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "IsInCombat", story: "Agent is in combat", category: "Conditions", id: "8b58e08466dd7820b7f330e13869fdf1")]
public partial class IsInCombatCondition : Condition
{

    public override bool IsTrue()
    {
        return true;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
