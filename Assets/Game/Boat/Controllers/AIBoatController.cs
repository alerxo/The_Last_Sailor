using System.Collections.Generic;
using UnityEngine;

public class AIBoatController : MonoBehaviour
{
    private const float APROACH_DISTANCE = 10f;
    private const float ENGAGEMENT_RANGE = 50f;

    private Boat boat;
    private List<Cannon> leftCannons, rightCannons;

    public Boat Target { get; private set; }
    private Vector3? destination;
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

    private void Boat_OnDestroyed()
    {
        ObjectPoolManager.Instance.Release(this);
    }

    private void Update()
    {
        if (Target == null) return;

        SetDestination();
        boat.ChangeMovement(GetMovementDirection());
        Fire();

#if UNITY_EDITOR
        if (isDebugMode)
        {
            DebugUtil.DrawBox(destination.Value, Quaternion.identity, Vector3.one, Color.green, Time.deltaTime);
        }
#endif
    }

    private Vector2 GetMovementDirection()
    {
        Vector2 movement = Vector2.zero;

        if (destination == null)
        {
            return movement;
        }

        distance = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(destination.Value.x, destination.Value.z));
        movement.y = distance < APROACH_DISTANCE ? distance / (APROACH_DISTANCE * 0.25f) : 1;

        cross = Vector3.Cross((transform.position - destination.Value).normalized, transform.forward);
        movement.x = cross.y;

        return movement;
    }

    public void SetDestination()
    {
        if (Vector3.Distance(Target.transform.position + (Target.transform.right * ENGAGEMENT_RANGE), transform.position) <
            Vector3.Distance(Target.transform.position - (Target.transform.right * ENGAGEMENT_RANGE), transform.position))
        {
            destination = Target.transform.position + (Target.transform.right * ENGAGEMENT_RANGE);
        }

        else
        {
            destination = Target.transform.position - (Target.transform.right * ENGAGEMENT_RANGE);
        }
    }

    public void SetTarget(Boat _target)
    {
        Target = _target;
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
}