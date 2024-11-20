using UnityEngine;

public class EnemyAdmiralController : Admiral
{
    public AIBoatController BoatController { get; private set; }

    public override void Awake()
    {
        base.Awake();

        BoatController = GetComponent<AIBoatController>();
        BoatController.SetAdmiral(this);
    }
}
