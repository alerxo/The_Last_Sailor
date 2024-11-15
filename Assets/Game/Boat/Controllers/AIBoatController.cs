using System.Collections.Generic;
using UnityEngine;

public class AIBoatController : MonoBehaviour
{
    private const float APROACH_DISTANCE = 10f;
    private const float ENGAGEMENT_RANGE = 50f;
    private const float ARRIVAL_RANGE = 10f;
    private const float DESTRUCTION_COOLDOWN = 1f;

    private AIBoatControllerState state;

    private Boat boat;
    private List<Cannon> leftCannons, rightCannons;

    public Boat Target { get; private set; }
    public Vector3? Destination { get; private set; }
    private float distance;
    private Vector3 cross;

    private float destructionTimer;

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
        SetState(AIBoatControllerState.Active);
    }

    public void OnDisable()
    {
        SetState(AIBoatControllerState.Disabled);
    }

    private void Update()
    {
        switch (state)
        {
            case AIBoatControllerState.Active:
                ActiveState();
                break;

            case AIBoatControllerState.PendingDestruction:
                PendingDestructionState();
                break;
        }
    }

    private void PendingDestructionState()
    {
        if ((destructionTimer -= Time.deltaTime) <= 0)
        {
            SetState(AIBoatControllerState.Destruction);
        }
    }

    private void ActiveState()
    {
        if (Target != null)
        {
            SetDestinationAtTarget();
            Fire();
        }

        else if (Destination != null)
        {
            CheckIfArrived();
        }

        SetMovement();

#if UNITY_EDITOR
        if (isDebugMode && Destination != null)
        {
            DebugUtil.DrawBox(Destination.Value, Quaternion.identity, Vector3.one, Target == null ? Color.green : Color.red, Time.deltaTime);
            Debug.DrawLine(transform.position, Destination.Value, Target == null ? Color.green : Color.red, Time.deltaTime);
        }
#endif
    }

    private void SetMovement()
    {
        if (Destination == null)
        {
            distance = 0;
            cross = Vector3.zero;
            boat.Engine.ChangeThrottle(0);
        }

        else
        {
            distance = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(Destination.Value.x, Destination.Value.z));
            boat.Engine.ChangeThrottle(distance < APROACH_DISTANCE ? distance / (APROACH_DISTANCE * 0.25f) : 1);
            cross = Vector3.Cross((transform.position - Destination.Value).normalized, transform.forward);
            boat.Engine.ChangeRudder(cross.y);
        }
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

    private void Boat_OnDestroyed()
    {
        CombatManager.Instance.RemoveBoat(this);
        Target = null;
        Destination = null;

        StartCoroutine(boat.SinkAtSurface(OnSunkAtSurface));
    }

    private void OnSunkAtSurface()
    {
        StartCoroutine(boat.SinkToBottom(OnSunkAtBottom));
    }

    private void OnSunkAtBottom()
    {
        SetState(AIBoatControllerState.PendingDestruction);
    }

    private void SetState(AIBoatControllerState _state)
    {
        state = _state;

        switch (state)
        {
            case AIBoatControllerState.Disabled:
                destructionTimer = DESTRUCTION_COOLDOWN;
                break;

            case AIBoatControllerState.Active:
                CombatManager.Instance.AddBoat(this);
                break;

            case AIBoatControllerState.PendingDestruction:
                CombatManager.Instance.RemoveBoat(this);
                boat.SetDefault();
                boat.Buoyancy.SetDefault();
                boat.RigidBody.linearVelocity = Vector3.zero;
                boat.RigidBody.angularVelocity = Vector3.zero;
                Target = null;
                Destination = null;
                break;

            case AIBoatControllerState.Destruction:
                ObjectPoolManager.Instance.Release(this);
                break;
        }
    }
}

public enum AIBoatControllerState
{
    Disabled,
    Active,
    PendingDestruction,
    Destruction
}