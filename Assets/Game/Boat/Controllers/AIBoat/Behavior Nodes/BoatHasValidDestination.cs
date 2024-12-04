using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "BoatHasValidDestination", story: "[Agent] has a valid destination", category: "Action", id: "4d53ff69fc86e9981d77278829d3e609")]
public partial class IsAtDestinationAction : Action
{
    [SerializeReference] public BlackboardVariable<AIBoatController> Agent;
    

    protected override Status OnStart()
    {


        

        return  Status.Success;
    }


}