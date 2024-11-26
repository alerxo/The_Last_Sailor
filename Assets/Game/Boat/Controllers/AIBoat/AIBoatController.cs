using UnityEngine;

public class AIBoatController : MonoBehaviour
{
    private const float DESTRUCTION_COOLDOWN = 1f;

    private AIBoatControllerState state;

    public Boat Boat { get; private set; }
    public Admiral Admiral { get; private set; }

    public Vector3? Destination { get; private set; }
    public float Speed { get; private set; } = 1f;
    public float Distance { get; private set; }
    public float ForwardCollisionDistance { get; private set; }
    public Vector3 Cross { get; private set; }

    private float destructionTimer;

    [SerializeField] private Transform[] rays;
    private float forwardCollisionTimer;
    private const float FORWARD_COLLISION_COOLDOWN = 1f;

#if UNITY_EDITOR
    [SerializeField] protected bool isDebugMode;
#endif

    public void Awake()
    {
        forwardCollisionTimer = Random.Range(0, FORWARD_COLLISION_COOLDOWN);

        Boat = GetComponent<Boat>();

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
                DrawDebug();
                break;

            case AIBoatControllerState.PendingDestruction:
                PendingDestructionState();
                break;
        }

        if ((forwardCollisionTimer += Time.deltaTime) > FORWARD_COLLISION_COOLDOWN)
        {
            forwardCollisionTimer = 0f;
            CheckForwardCollision();
        }
    }

    private void CheckForwardCollision()
    {
        float distance = Distance;

        foreach (Transform ray in rays)
        {
            if (Physics.Raycast(ray.position, ray.forward, out RaycastHit hit, MoveTowardsDestination.APROACH_DISTANCE))
            {
                distance = Mathf.Min(hit.distance, distance);

#if UNITY_EDITOR
                if (isDebugMode)
                {
                    Debug.DrawLine(ray.position, hit.point, Color.yellow, FORWARD_COLLISION_COOLDOWN);
                }
#endif
            }
        }

        ForwardCollisionDistance = distance;
    }

    public void Seize(Admiral _admiral)
    {
        if (GetComponentInChildren<EnemyAdmiralController>() != null)
        {
            DemoteFromAdmiral();
        }

        else
        {
            Admiral.RemoveSubordinate(Boat);
        }

        _admiral.AddSubordinate(Boat);
        Boat.Repair();
    }

    public EnemyAdmiralController PromoteToAdmiral()
    {
        EnemyAdmiralController admiralController = ObjectPoolManager.Instance.Spawn<EnemyAdmiralController>(transform.position, transform.rotation, transform);
        admiralController.SetOwner(Boat);
        admiralController.SetController(this);
        SetAdmiral(admiralController);

        return admiralController;
    }

    public void DemoteFromAdmiral()
    {
        EnemyAdmiralController admiralController = GetComponentInChildren<EnemyAdmiralController>();
        admiralController.SetController(null);
        admiralController.RemoveOwner();
        SetAdmiral(null);
        ObjectPoolManager.Instance.Release(admiralController);
    }

    public void SetAdmiral(Admiral _admiral)
    {
        Admiral = _admiral;
    }

    public void SetDestination(Vector3 _destination)
    {
        Destination = _destination;
    }

    public void SetSpeed(float _speed)
    {
        Speed = _speed;
    }

    public void SetDistance(float _distance)
    {
        Distance = _distance;
    }

    public void SetCross(Vector3 _cross)
    {
        Cross = _cross;
    }

    private void Boat_OnDestroyed()
    {
        Destination = null;
        Boat.StartSinkAtSurface();
    }

    public void SinkToBottom()
    {
        if (GetComponentInChildren<EnemyAdmiralController>() != null)
        {
            DemoteFromAdmiral();
        }

        else
        {
            Admiral.RemoveSubordinate(Boat);
            SetAdmiral(null);
        }

        Boat.StartSinkToBottom(OnSunkAtBottom);
    }

    private void OnSunkAtBottom()
    {
        SetState(AIBoatControllerState.PendingDestruction);
    }

    private void PendingDestructionState()
    {
        if ((destructionTimer -= Time.deltaTime) <= 0)
        {
            SetState(AIBoatControllerState.Destruction);
        }
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
                break;

            case AIBoatControllerState.PendingDestruction:
                Boat.Repair();
                Boat.RigidBody.linearVelocity = Vector3.zero;
                Boat.RigidBody.angularVelocity = Vector3.zero;
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

    private void DrawDebug()
    {
#if UNITY_EDITOR
        if (isDebugMode && Destination != null)
        {
            DebugUtil.DrawBox(Destination.Value, Quaternion.identity, Vector3.one, Admiral != null && Admiral.Enemy == null ? Color.green : Color.red, Time.deltaTime);
            Debug.DrawLine(transform.position, Destination.Value, Admiral != null && Admiral.Enemy == null ? Color.green : Color.red, Time.deltaTime);
        }
#endif
    }
}

public enum AIBoatControllerState
{
    Disabled,
    Active,
    PendingDestruction,
    Destruction
}