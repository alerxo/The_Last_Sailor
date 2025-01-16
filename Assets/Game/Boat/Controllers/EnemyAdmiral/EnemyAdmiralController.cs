using Unity.Behavior;
using UnityEngine;

public class EnemyAdmiralController : Admiral
{
    private BehaviorGraphAgent behaviourAgent;
    public AIBoatController AIBoatController { get; private set; }

    private const float MAX_ACCEPTABLE_LONGEST_SUBORDINATE_DISTANCE = 200f;
    private const float ACCEPTABLE_LONGEST_SUBORDINATE_MARGIN = 10f;
    private const float WAIT_ON_SUBORDINATE_CATCHUP_SPEED = 0.3f;

    private float longestSubordinateDistance;

    private Formation formation;

    public void Awake()
    {
        behaviourAgent = GetComponent<BehaviorGraphAgent>();
        SetName("Admiral Johnson");
    }

    public void SetController(AIBoatController _controller)
    {
        AIBoatController = _controller;
        behaviourAgent.GetVariable("BoatController", out BlackboardVariable variable);
        variable.ObjectValue = AIBoatController;
    }

    public void SpawnSubordinate(Vector3 _formation, Quaternion _rotation)
    {
        Vector3 position = BuoyancyManager.Instance.GetPointOnWater(transform.position + transform.TransformVector(_formation));
;
        AIBoatController subordinate = ObjectPoolManager.Instance.Spawn<AIBoatController>(position, _rotation);
        subordinate.LerpSize();
        AddSubordinate(subordinate.Boat);
        subordinate.Boat.SetName(GetSubordinateName());
        subordinate.SetFormationPosition(_formation);
    }

    public void SetDestination(Vector3 _position)
    {
        SetLongestSubordínateDistance();
        AIBoatController.SetDestination(_position);
        float distance = Mathf.Clamp(longestSubordinateDistance - BoatMovesTowardsDestination.APROACH_DISTANCE - ACCEPTABLE_LONGEST_SUBORDINATE_MARGIN, 0, MAX_ACCEPTABLE_LONGEST_SUBORDINATE_DISTANCE);
        AIBoatController.SetSpeed(Mathf.Lerp(1f, WAIT_ON_SUBORDINATE_CATCHUP_SPEED, distance / MAX_ACCEPTABLE_LONGEST_SUBORDINATE_DISTANCE));
    }

    private void SetLongestSubordínateDistance()
    {
        float distance = 0;

        for (int i = 0; i < Subordinates.Count; i++)
        {
            distance = Mathf.Max(distance, Subordinates[i].Distance);
        }

        longestSubordinateDistance = distance;
    }

    public void GetRandomFormation()
    {
        formation = (Formation)Random.Range(0, 3);
    }

    public Vector3[] GetFormationPositions(int _size)
    {
        return Formations.GetFleetPositions(formation, _size);
    }

    public override string GetSubordinateName()
    {
        return $"Enemy Boat {Subordinates.Count}";
    }

    public bool IsSunk() => AIBoatController == null;
}
