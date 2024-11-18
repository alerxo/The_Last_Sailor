using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "IsInRingOfFire", story: "Agent is inside ring of fire", category: "Conditions", id: "8a70ccd729820c5cdfb796f0f44cc03c")]
public partial class IsInRingOfFireCondition : Condition
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
