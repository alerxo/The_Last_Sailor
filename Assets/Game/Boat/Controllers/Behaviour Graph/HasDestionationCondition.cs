using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "HasDestionation", story: "Agent has a destination", category: "Conditions", id: "19057b07335820f3fc4d31ce86880a59")]
public partial class HasDestionationCondition : Condition
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
