using System.Collections.Generic;
using UnityEngine;

public class AIBoatController : MonoBehaviour
{
    private const float APROACH_DISTANCE = 10f;
    private const float ENGAGEMENT_RANGE = 50f;
    private const float ARRIVAL_RANGE = 10f;

    private Boat boat;
    private List<Cannon> leftCannons, rightCannons;

    public Boat Target { get; private set; }
    public Vector3? Destination { get; private set; }
    private float distance;
    private Vector3 cross;

#if UNITY_EDITOR
    [SerializeField] protected bool isDebugMode;
#endif

    public void Awake()
    {
        boat = GetComponent<Boat>();

        leftCannons = new();
        rightCannons = new();

        foreach (Cannon cannon in GetComponentsInChildren<Cannon>())
        {
            if (cannon.transform.localPosition.x > 0) rightCannons.Add(cannon);
            else leftCannons.Add(cannon);
        }

        boat.OnDestroyed += Boat_OnDestroyed;
    }

    public void OnEnable()
    {
        CombatManager.Instance.AddBoat(this);
    }

    public void OnDisable()
    {
        CombatManager.Instance.RemoveBoat(this);
    }

    private void Boat_OnDestroyed()
    {
        ObjectPoolManager.Instance.Release(this);
    }

    private void Update()
    {
        boat.ChangeMovement(GetMovementDirection());

        if (Target == null)
        {
            CheckIfArrived();
        }

        else
        {
            SetDestinationAtTarget();
            Fire();
        }

#if UNITY_EDITOR
        if (isDebugMode && Destination != null)
        {
            DebugUtil.DrawBox(Destination.Value, Quaternion.identity, Vector3.one, Target == null ? Color.green : Color.red, Time.deltaTime);
            Debug.DrawLine(transform.position, Destination.Value, Target == null ? Color.green : Color.red, Time.deltaTime);
        }
#endif
    }

    private Vector2 GetMovementDirection()
    {
        Vector2 movement = Vector2.zero;

        if (Destination == null)
        {
            return movement;
        }

        distance = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(Destination.Value.x, Destination.Value.z));
        movement.y = distance < APROACH_DISTANCE ? distance / (APROACH_DISTANCE * 0.25f) : 1;

        cross = Vector3.Cross((transform.position - Destination.Value).normalized, transform.forward);
        movement.x = cross.y;

        return movement;
    }

    public void SetDestinationAtTarget()
    {
        if (Vector3.Distance(Target.transform.position + (Target.transform.right * ENGAGEMENT_RANGE), transform.position) <
            Vector3.Distance(Target.transform.position - (Target.transform.right * ENGAGEMENT_RANGE), transform.position))
        {
            SetDestination(Target.transform.position + (Target.transform.right * ENGAGEMENT_RANGE));
        }

        else
        {
            SetDestination(Target.transform.position - (Target.transform.right * ENGAGEMENT_RANGE));
        }
    }

    private void Fire()
    {
        if (Vector3.Cross((transform.position - Target.transform.position).normalized, transform.forward).y < 0)
        {
            Fire(leftCannons);
        }

        else
        {
            Fire(rightCannons);
        }
    }

    private void Fire(List<Cannon> _cannons)
    {
        foreach (Cannon cannon in _cannons)
        {
            if (cannon.State == CannonState.Ready)
            {
                cannon.Fire();
            }
        }
    }

    private void CheckIfArrived()
    {
        if (Vector3.Distance(transform.position, Destination.Value) < ARRIVAL_RANGE)
        {
            Destination = null;
        }
    }

    public void SetDestination(Vector3 _destination)
    {
        Destination = _destination;
    }

    public void SetTarget(Boat _target)
    {
        Target = _target;
    }
}