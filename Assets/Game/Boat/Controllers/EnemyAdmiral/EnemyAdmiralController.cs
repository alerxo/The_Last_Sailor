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

    private void OnEnable()
    {
        CombatManager.Instance.AddAdmiral(this);
    }

    private void OnDisable()
    {
        CombatManager.Instance.RemoveAdmiral(this);
    }
}
