using UnityEngine;

public class AIBoatController : MonoBehaviour
{
    public AIBoatControllerState State { get; private set; }

    public Boat Boat { get; private set; }
    public Admiral Admiral { get; private set; }

    [SerializeField] private Transform[] rays;
    private float forwardCollisionTimer;
    private const float FORWARD_COLLISION_COOLDOWN = 1f;
    public float ForwardCollisionDistance { get; private set; }

    public Command Command { get; private set; } = Command.Unassigned;
    public Vector3? FormationPosition { get; private set; }
    public Vector3? Destination { get; private set; }
    public float Speed { get; private set; } = 1f;
    public float Distance { get; private set; }

    private float destructionTimer;
    private const float DESTRUCTION_COOLDOWN = 1f;

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
        switch (State)
        {
            case AIBoatControllerState.Active:
                TryCheckForCollisions();
                DrawDebug();
                break;

            case AIBoatControllerState.PendingDestruction:
                PendingDestructionState();
                break;
        }
    }

    private void TryCheckForCollisions()
    {
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
            if (Physics.Raycast(ray.position, ray.forward, out RaycastHit hit, BoatMovesTowardsDestination.APROACH_DISTANCE))
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

    public Vector3 GetFormationPositionInWorld()
    {
        return Admiral.transform.position + Admiral.transform.TransformVector(FormationPosition.Value);
    }

    private void Boat_OnDestroyed()
    {
        Destination = null;
        FormationPosition = null;
        Boat.StartSinkAtSurface();
    }

    public void Seize(Admiral _admiral)
    {
        RemoveFromFleet();
        Boat.SetName(PlayerBoatController.Instance.AdmiralController.GetSubordinateName());
        Boat.Repair();
        SetCommand(Command.Unassigned);
        _admiral.AddSubordinate(Boat);
    }

    public void Scrap()
    {
        SinkToBottom();
    }

    public void SinkToBottom()
    {
        RemoveFromFleet();
        SetState(AIBoatControllerState.SinkingToBottom);
    }

    private void RemoveFromFleet()
    {
        if (GetComponentInChildren<EnemyAdmiralController>() != null)
        {
            DemoteFromAdmiral();
        }

        else
        {
            Admiral.RemoveSubordinate(Boat);
        }
    }

    public EnemyAdmiralController PromoteToAdmiral()
    {
        EnemyAdmiralController admiralController = ObjectPoolManager.Instance.Spawn<EnemyAdmiralController>(transform.position, transform.rotation, transform);
        admiralController.SetOwner(Boat);
        admiralController.SetController(this);
        SetAdmiral(admiralController);
        SetCommand(Command.Unassigned);
        Boat.SetName($"{admiralController.Name}'s Boat");

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

    private void PendingDestructionState()
    {
        if ((destructionTimer -= Time.deltaTime) <= 0)
        {
            SetState(AIBoatControllerState.Destruction);
        }
    }

    private void SetState(AIBoatControllerState _state)
    {
        State = _state;

        switch (State)
        {
            case AIBoatControllerState.Disabled:
                destructionTimer = DESTRUCTION_COOLDOWN;
                break;

            case AIBoatControllerState.Active:
                break;

            case AIBoatControllerState.SinkingToBottom:
                Boat.StartSinkToBottom(() => SetState(AIBoatControllerState.PendingDestruction));
                break;

            case AIBoatControllerState.PendingDestruction:
                Boat.Repair();
                Boat.RigidBody.linearVelocity = Vector3.zero;
                Boat.RigidBody.angularVelocity = Vector3.zero;
                ClearDestination();
                break;

            case AIBoatControllerState.Destruction:
                ObjectPoolManager.Instance.Release(this);

                break;
        }
    }

    public void SetAdmiral(Admiral _admiral)
    {
        Admiral = _admiral;
    }

    public void SetCommand(Command _command)
    {
        Command = _command;
    }

    public void SetFormationPosition(Vector3 _position)
    {
        FormationPosition = _position;
    }

    public void SetDestination(Vector3 _destination)
    {
        Destination = _destination;
    }

    public void ClearDestination()
    {
        Destination = null;
    }

    public void SetSpeed(float _speed)
    {
        Speed = _speed;
    }

    public void SetDistance(float _distance)
    {
        Distance = _distance;
    }

    private void DrawDebug()
    {
#if UNITY_EDITOR
        if (isDebugMode && Destination != null)
        {
            Color color = Color.green;
            if (Admiral == null) color = Color.yellow;
            else if (Admiral.Enemy != null) color = Color.red;

            DebugUtil.DrawBox(Destination.Value, Quaternion.identity, Vector3.one, color, Time.deltaTime);
            Debug.DrawLine(transform.position, Destination.Value, color, Time.deltaTime);
        }
#endif
    }
}

public enum AIBoatControllerState
{
    Disabled,
    Active,
    SinkingToBottom,
    PendingDestruction,
    Destruction
}