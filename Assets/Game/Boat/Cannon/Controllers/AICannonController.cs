using UnityEngine;

public class AICannonController : MonoBehaviour
{
    private const float MAX_RANGE = 300f;
    private const float MAX_AIM_ANGLE = 160f;
    private const float MAX_FIRE_ANGLE = 3f;
    private const float BOAT_LENGTH = 6f;

    private AICannonControllerState state;

    private AIBoatController owner;
    private Cannon cannon;
    private Boat target;

#if UNITY_EDITOR
    [SerializeField] private bool isDebugMode;
#endif

    private void Awake()
    {
        cannon = GetComponentInParent<Cannon>();
        owner = GetComponentInParent<AIBoatController>();
    }

    private void Update()
    {
        CheckIfReady();

        if (state == AICannonControllerState.Targeting)
        {
            TryGetTarget();
        }

        if (state == AICannonControllerState.Aiming)
        {
            AimAtTarget();
        }

        if (state == AICannonControllerState.Shooting)
        {
            FireAtTarget();
        }
    }

    private void CheckIfReady()
    {
        if (cannon.State != CannonState.Ready || owner.Admiral.Enemy == null || owner.Boat.Health <= 0)
        {
            SetState(AICannonControllerState.Inactive);
        }

        else
        {
            SetState(AICannonControllerState.Targeting);
        }
    }

    private void TryGetTarget()
    {
        float closest = float.MaxValue;
        target = null;

        foreach (Boat boat in owner.Admiral.Enemy.Fleet)
        {
            float distance = Vector3.Distance(transform.position, boat.transform.position);

            if (distance < closest && IsValidTarget(boat, distance))
            {
                target = boat;
                closest = distance;
            }
        }

        if (target != null)
        {
            SetState(AICannonControllerState.Aiming);
        }
    }

    private bool IsValidTarget(Boat _boat, float _distance)
    {
        return _distance < MAX_RANGE && GetCurrentAngle(_boat) <= MAX_AIM_ANGLE;
    }

    private void AimAtTarget()
    {
        RotatePitch();
        RotateYaw();

        if (GetCurrentAngle(target) < GetAimWindowAngle(target) + MAX_FIRE_ANGLE)
        {
            SetState(AICannonControllerState.Shooting);
        }
    }

    private void RotatePitch()
    {
        float distance = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(target.transform.position.x, target.transform.position.z));
        cannon.SetPitch(Mathf.Lerp(-1, 1, distance / MAX_RANGE));
    }

    private void RotateYaw()
    {
        cannon.SetYaw(Vector3.Cross((transform.position - target.transform.position).normalized, transform.forward).y);
    }

    private void FireAtTarget()
    {
        cannon.Fire();
    }

    private float GetCurrentAngle(Boat _boat)
    {
#if UNITY_EDITOR
        if (isDebugMode)
        {
            float distance = Vector3.Distance(transform.position, _boat.transform.position) + BOAT_LENGTH;
            Debug.DrawLine(transform.position, transform.position + (transform.forward * distance), Color.yellow, Time.deltaTime);
            Debug.DrawLine(transform.position, transform.position + (_boat.transform.position - transform.position).normalized * distance, Color.green, Time.deltaTime);
        }
#endif
        return Vector3.Angle(transform.forward, (_boat.transform.position - transform.position).normalized);
    }

    private float GetAimWindowAngle(Boat _boat)
    {
#if UNITY_EDITOR
        if (isDebugMode)
        {
            float distance = Vector3.Distance(transform.position, _boat.transform.position) + BOAT_LENGTH;
            Debug.DrawLine(transform.position, transform.position + ((GetOffsetPosition(_boat, BOAT_LENGTH) - transform.position).normalized * distance), Color.red, Time.deltaTime);
            Debug.DrawLine(transform.position, transform.position + ((GetOffsetPosition(_boat, -BOAT_LENGTH) - transform.position).normalized * distance), Color.red, Time.deltaTime);
        }
#endif
        return Vector3.Angle((GetOffsetPosition(_boat, BOAT_LENGTH) - transform.position).normalized, (GetOffsetPosition(_boat, -BOAT_LENGTH) - transform.position).normalized) / 2;
    }

    private Vector3 GetOffsetPosition(Boat _boat, float _offset)
    {
        return _boat.transform.position + _boat.transform.TransformVector(new Vector3(0, 0, _offset));
    }

    private void SetState(AICannonControllerState _state)
    {
        state = _state;
    }
}

public enum AICannonControllerState
{
    Inactive,
    Targeting,
    Aiming,
    Shooting
}