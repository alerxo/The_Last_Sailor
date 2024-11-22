using UnityEngine;

public class EnemyAdmiralController : Admiral
{
    public AIBoatController BoatController { get; private set; }

    private const float MAX_ACCEPTABLE_LONGEST_SUBORDINATE_DISTANCE = 300f;
    private const float ACCEPTABLE_LONGEST_SUBORDINATE_MARGIN = 10f;
    private const float WAIT_ON_SUBORDINATE_CATCHUP_SPEED = 0.5f;

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

    public void SpawnSubordinate(Vector3 position)
    {
        AIBoatController subordinate = ObjectPoolManager.Instance.Spawn<AIBoatController>(position, transform.rotation);
        subordinate.SetAdmiral(this);
        AddSubordinate(subordinate.Boat);
    }

    public void SetDestination(Vector3 position)
    {
        BoatController.SetDestination(position);
        float distance = Mathf.Clamp(LongestSubordinateDistance - MoveTowardsDestination.APROACH_DISTANCE - ACCEPTABLE_LONGEST_SUBORDINATE_MARGIN, 0, MAX_ACCEPTABLE_LONGEST_SUBORDINATE_DISTANCE);
        BoatController.SetSpeed(Mathf.Lerp(1f, WAIT_ON_SUBORDINATE_CATCHUP_SPEED, distance / MAX_ACCEPTABLE_LONGEST_SUBORDINATE_DISTANCE));
    }
}
