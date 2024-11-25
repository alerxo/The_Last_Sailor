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
    public Vector3 Cross { get; private set; }

    private float destructionTimer;

#if UNITY_EDITOR
    [SerializeField] protected bool isDebugMode;
#endif

    public void Awake()
    {
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
        Boat.NavigationObstacle.TryStopOccupying();

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

                if (GetComponent<EnemyAdmiralController>() == null)
                {
                    Admiral.RemoveSubordinate(Boat);
                    SetAdmiral(null);
                }

                Boat.SetDefault();
                Boat.Buoyancy.SetDefault();
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
            DebugUtil.DrawBox(Destination.Value, Quaternion.identity, Vector3.one, Admiral.Enemy == null ? Color.green : Color.red, Time.deltaTime);
            Debug.DrawLine(transform.position, Destination.Value, Admiral.Enemy == null ? Color.green : Color.red, Time.deltaTime);
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