using System.Collections.Generic;
using UnityEngine;

public class AIBoatController : MonoBehaviour
{
    public AIBoatControllerState State { get; private set; }

    public Boat Boat { get; private set; }
    public Admiral Admiral { get; private set; }

    public Command Command { get; private set; } = Command.Unassigned;
    public Vector3? FormationPosition { get; private set; }
    public Vector3? HoldPosition { get; private set; }

    private Vector3? Destination;
    private readonly List<Vector3> Trail = new();
    public const int TRAIL_DISTANCE = 50;
    private const int MIN_TRAIL_ANGLE = 90;
    private int maxTrailCount = 0;

    public Transform[] CollisionRays;

    public float Speed { get; private set; } = 1f;
    public float Distance { get; private set; }

    private float destructionTimer;
    private const float DESTRUCTION_COOLDOWN = 1f;

#if UNITY_EDITOR
    public bool IsDebugMode;
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
        switch (State)
        {
            case AIBoatControllerState.Active:
                SetDistance();
                UpdateTrail();
                DebugDrawTrail();
                break;

            case AIBoatControllerState.PendingDestruction:
                PendingDestructionState();
                break;
        }

        Boat.Engine.SetOverCharge(Admiral != null && Admiral.Enemy != null ? 1f : 1.2f);
    }

    private void SetDistance()
    {
        Distance = Destination.HasValue
            ? Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(Destination.Value.x, Destination.Value.z))
            : 0;
    }

    private void UpdateTrail()
    {
        if (Trail.Count > maxTrailCount || (Trail.Count > 0 && Vector3.Distance(transform.position, Trail[0]) < TRAIL_DISTANCE))
        {
            ConsumeTrail();
            return;
        }

        if (Trail.Count > 2 && HasBadTrail())
        {
            Trail.RemoveRange(0, 2);
            return;
        }

        if (!HasDestination() || maxTrailCount == 0) return;
        if (Trail.Count == 0 && Vector3.Distance(Destination.Value, transform.position) < BoatMovesTowardsDestination.STOP_DISTANCE) return;
        if (Trail.Count > 0 && Vector3.Distance(Destination.Value, Trail[^1]) < TRAIL_DISTANCE) return;

        if (Vector3.Distance(transform.position, Destination.Value) > TRAIL_DISTANCE)
        {
            Trail.Add(Destination.Value);
        }
    }

    private bool HasBadTrail()
    {
        return Vector3.Angle((transform.position - Trail[0]).normalized, (Trail[1] - Trail[0]).normalized) < MIN_TRAIL_ANGLE;
    }

    public Vector3 GetFormationPositionInWorld()
    {
        return GetPositionRelativeToAdmiral(FormationPosition.Value);
    }

    public Vector3 GetPositionRelativeToAdmiral(Vector3 _position)
    {
        return Admiral.transform.position + Admiral.transform.TransformVector(_position);
    }

    private void Boat_OnDestroyed()
    {
        SetFormationPosition(null);
        SetHoldPosition(null);
        ClearTrail();
        TrySetCommand(Command.Unassigned);
        Boat.StartSinkAtSurface();
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
        TrySetCommand(Command.Unassigned);
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
                Boat.ResetUpgrades();
                Boat.Repair();
                Boat.RigidBody.linearVelocity = Vector3.zero;
                Boat.RigidBody.angularVelocity = Vector3.zero;
                break;

            case AIBoatControllerState.Destruction:

                if (TryGetComponent(out AIBoatController_Allied ally)) ObjectPoolManager.Instance.Release(ally);
                else ObjectPoolManager.Instance.Release(this);

                break;
        }
    }

    public void TrySetCommand(Command _command)
    {
        if (Boat.IsSunk) return;

        switch (_command)
        {
            case Command.Unassigned:
                SetCommand(Command.Unassigned);
                break;

            case Command.Follow when FormationPosition.HasValue:
                SetCommand(Command.Follow);
                break;

            case Command.Wait when FormationPosition.HasValue:
                SetCommand(Command.Wait);
                break;

            case Command.Charge:
                SetCommand(Command.Charge);
                break;

            default:
                SetCommand(Command.Unassigned);
                break;
        }
    }

    private void SetCommand(Command _command)
    {
        if (_command == Command) return;

        switch (_command)
        {
            case Command.Unassigned:
                SetHoldPosition(null);
                SetMaxTrail(0);
                ClearTrail();
                break;

            case Command.Follow:
                SetHoldPosition(null);
                SetMaxTrail(5);
                ClearTrail();
                break;

            case Command.Wait:
                SetHoldPosition(GetFormationPositionInWorld());
                SetMaxTrail(5);
                ClearTrail();
                break;

            case Command.Charge:
                SetHoldPosition(null);
                SetMaxTrail(0);
                ClearTrail();
                break;
        }

        Command = _command;
    }

    public void SetAdmiral(Admiral _admiral)
    {
        Admiral = _admiral;
        Boat.SetCannonBallOwner(_admiral == PlayerBoatController.Instance.AdmiralController ? CannonballOwner.Allied : CannonballOwner.Enemy);
    }

    public void SetFormationPosition(Vector3? _position)
    {
        ClearTrail();
        FormationPosition = _position;
    }

    public void SetHoldPosition(Vector3? _position)
    {
        HoldPosition = _position;
    }

    public void SetDestination(Vector3 _destination)
    {
        _destination.y = 5;

        if (Destination.HasValue && Vector3.Distance(_destination, Destination.Value) < 25) return;

        Destination = _destination;
    }

    public bool HasDestination()
    {
        return Destination.HasValue;
    }

    public bool HasTrail()
    {
        return Trail.Count > 0 || HasDestination();
    }

    public bool HasNextTrail()
    {
        return Trail.Count > 1;
    }

    public Vector3 GetCurrentTrail()
    {
        return Trail.Count > 0 ? Trail[0] : Destination.Value;
    }

    public Vector3 GetNextTrail()
    {
        return Trail[0];
    }

    public void ConsumeTrail()
    {
        if (Trail.Count > 0)
        {
            Trail.RemoveAt(0);
        }
    }

    public void ClearTrail()
    {
        Trail.Clear();
        Destination = null;
    }

    public void SetMaxTrail(int _count)
    {
        maxTrailCount = _count;
    }

    public void SetSpeed(float _speed)
    {
        Speed = _speed;
    }

    private void DebugDrawTrail()
    {
#if UNITY_EDITOR
        if (IsDebugMode && HasTrail())
        {
            Color color = Admiral == null ? Color.yellow : (Admiral.Enemy == null ? Color.green : Color.red);
            Vector3 last = transform.position;

            foreach (Vector3 point in Trail)
            {
                DebugUtil.DrawBox(point, Quaternion.identity, Vector3.one, color, Time.deltaTime);
                Debug.DrawLine(last, point, color, Time.deltaTime);
                last = point;
            }

            Debug.DrawLine(last, Destination.Value, color, Time.deltaTime);
            DebugUtil.DrawBox(Destination.Value, Quaternion.identity, Vector3.one * 2, color, Time.deltaTime);
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