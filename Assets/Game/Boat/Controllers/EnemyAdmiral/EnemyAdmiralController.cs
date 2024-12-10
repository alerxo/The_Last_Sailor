using Unity.Behavior;
using UnityEngine;

public class EnemyAdmiralController : Admiral
{
    private BehaviorGraphAgent behaviourAgent;
    public AIBoatController AIBoatController { get; private set; }

    private const float MAX_ACCEPTABLE_LONGEST_SUBORDINATE_DISTANCE = 300f;
    private const float ACCEPTABLE_LONGEST_SUBORDINATE_MARGIN = 10f;
    private const float WAIT_ON_SUBORDINATE_CATCHUP_SPEED = 0.5f;

    private float longestSubordinateDistance;

    private Formation formation;

    private void OnEnable()
    {
        CombatManager.Instance.AddAdmiral(this);
    }

    private void OnDisable()
    {
        CombatManager.Instance.RemoveAdmiral(this);
    }

    public void Awake()
    {
        behaviourAgent = GetComponent<BehaviorGraphAgent>();
        SetName("Admiral Johnson");
    }

    public void SetController(AIBoatController controller)
    {
        AIBoatController = controller;
        behaviourAgent.GetVariable("BoatController", out BlackboardVariable variable);
        variable.ObjectValue = AIBoatController;
    }

    public void SpawnSubordinate(Vector3 position)
    {
        AIBoatController subordinate = ObjectPoolManager.Instance.Spawn<AIBoatController>(position, transform.rotation);
        AddSubordinate(subordinate.Boat);
        subordinate.Boat.SetName(GetSubordinateName());
    }

    public void SetDestination(Vector3 position)
    {
        AIBoatController.SetDestination(position);
        float distance = Mathf.Clamp(longestSubordinateDistance - BoatMovesTowardsDestination.APROACH_DISTANCE - ACCEPTABLE_LONGEST_SUBORDINATE_MARGIN, 0, MAX_ACCEPTABLE_LONGEST_SUBORDINATE_DISTANCE);
        AIBoatController.SetSpeed(Mathf.Lerp(1f, WAIT_ON_SUBORDINATE_CATCHUP_SPEED, distance / MAX_ACCEPTABLE_LONGEST_SUBORDINATE_DISTANCE));
    }

    public void GetRandomFormation()
    {
        formation = (Formation)Random.Range(0, 3);
    }

    public void SetFleetFormation()
    {
        Vector3[] positions = Formations.GetFleetPositions(formation, Subordinates.Count);

        float distance = 0;

        for (int i = 0; i < Subordinates.Count; i++)
        {
            if (i >= Subordinates.Count || i >= positions.Length)
            {
                break;
            }

            Subordinates[i].SetFormationPosition(positions[i]);
            distance = Mathf.Max(distance, Subordinates[i].Distance);
        }

        longestSubordinateDistance = distance;
    }

    public override string GetSubordinateName()
    {
        return $"Enemy Boat {Subordinates.Count}";
    }

    public bool IsSunk() => AIBoatController == null;
}
