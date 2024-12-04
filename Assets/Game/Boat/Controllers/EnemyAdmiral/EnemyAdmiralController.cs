using UnityEngine;

public class EnemyAdmiralController : Admiral
{
    public AIBoatController BoatController { get; private set; }

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
        formation = (Formation)Random.Range(0, 3);

        SetName("Admiral Johnson");
    }

    public void SetController(AIBoatController controller)
    {
        BoatController = controller;
    }

    public void SpawnSubordinate(Vector3 position)
    {
        AIBoatController subordinate = ObjectPoolManager.Instance.Spawn<AIBoatController>(position, transform.rotation);
        AddSubordinate(subordinate.Boat);
        subordinate.Boat.SetName(GetSubordinateName());
        subordinate.SetCommand(Command.Formation);
    }

    public void SetDestination(Vector3 position)
    {
        BoatController.SetDestination(position);
        float distance = Mathf.Clamp(longestSubordinateDistance - BoatMovesTowardsDestination.APROACH_DISTANCE - ACCEPTABLE_LONGEST_SUBORDINATE_MARGIN, 0, MAX_ACCEPTABLE_LONGEST_SUBORDINATE_DISTANCE);
        BoatController.SetSpeed(Mathf.Lerp(1f, WAIT_ON_SUBORDINATE_CATCHUP_SPEED, distance / MAX_ACCEPTABLE_LONGEST_SUBORDINATE_DISTANCE));
    }

    public void SetFleetFormation()
    {
        Vector3[] positions = GetFleetPositions();

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

    private Vector3[] GetFleetPositions()
    {
        switch (formation)
        {
            case Formation.Line:
                return Formations.GetLine(Subordinates.Count);

            case Formation.Spearhead:
                return Formations.GetSpearhead(Subordinates.Count);

            case Formation.Ring:
                return Formations.GetRing(Subordinates.Count);

            default:
                Debug.LogError($"Defaulted for case {formation}");
                return null;
        }
    }

    public override string GetSubordinateName()
    {
        return $"Enemy Boat {Subordinates.Count}";
    }
}
