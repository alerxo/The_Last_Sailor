using System.Collections.Generic;
using UnityEngine;

public class AIBoatController : MonoBehaviour
{
    private const float DESTRUCTION_COOLDOWN = 1f;

    private AIBoatControllerState state;

    public Boat Boat { get; private set; }
    private List<Cannon> leftCannons, rightCannons;

    public Boat Target { get; private set; }
    public Vector3? Destination { get; private set; }
    public float distance;
    public Vector3 cross;

    private float destructionTimer;

#if UNITY_EDITOR
    [SerializeField] protected bool isDebugMode;
#endif

    public void Awake()
    {
        Boat = GetComponent<Boat>();

        leftCannons = new();
        rightCannons = new();

        foreach (Cannon cannon in GetComponentsInChildren<Cannon>())
        {
            if (cannon.transform.localPosition.x > 0) rightCannons.Add(cannon);
            else leftCannons.Add(cannon);
        }

        Boat.OnDestroyed += Boat_OnDestroyed;
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

    private void ActiveState()
    {
        if (Target != null && Boat.Health > 0)
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

#if UNITY_EDITOR
        if (isDebugMode && Destination != null)
        {
            DebugUtil.DrawBox(Destination.Value, Quaternion.identity, Vector3.one, Target == null ? Color.green : Color.red, Time.deltaTime);
            Debug.DrawLine(transform.position, Destination.Value, Target == null ? Color.green : Color.red, Time.deltaTime);
        }
#endif
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

    public void SetDestination(Vector3 _destination)
    {
        Destination = _destination;
    }

    public void SetTarget(Boat _target)
    {
        Target = _target;
    }

    private void PendingDestructionState()
    {
        if ((destructionTimer -= Time.deltaTime) <= 0)
        {
            SetState(AIBoatControllerState.Destruction);
        }
    }

    private void Boat_OnDestroyed()
    {
        CombatManager.Instance.RemoveBoat(this);
        Target = null;
        Destination = null;

        StartCoroutine(Boat.SinkAtSurface(OnSunkAtSurface));
    }

    private void OnSunkAtSurface()
    {
        StartCoroutine(Boat.SinkToBottom(OnSunkAtBottom));
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
                Boat.SetDefault();
                Boat.Buoyancy.SetDefault();
                Boat.RigidBody.linearVelocity = Vector3.zero;
                Boat.RigidBody.angularVelocity = Vector3.zero;
                Target = null;
                Destination = null;
                break;

            case AIBoatControllerState.Destruction:

                if (TryGetComponent(out EnemyAdmiralController enemyAdmiralController))
                {
                    ObjectPoolManager.Instance.Release(enemyAdmiralController);
                }

                else
                {
                    ObjectPoolManager.Instance.Release(this);
                }

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