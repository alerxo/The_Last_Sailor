using UnityEngine;

public class AICannonController : MonoBehaviour
{
    private const float MAX_TARGET_ANGLE = 100f;
    private const float MAX_FIRE_PITCH_DIFFERENCE = 0.50f;
    private const float MAX_FIRE_YAW_DIFFERENCE = 1f;
    private const float BOAT_LENGTH = 15f;

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
        CheckIfActive();

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

    private void CheckIfActive()
    {
        if (owner.Admiral.Enemy == null || owner.Boat.Health <= 0)
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
        return _distance < CombatManager.RING_OF_FIRE_SIZE && GetCurrentAngle(_boat.transform.position) <= MAX_TARGET_ANGLE;
    }

    private void AimAtTarget()
    {
        Vector3 targetPredictedPosition = GetPredictedPosition();

        bool pitch = RotatePitch(targetPredictedPosition);
        bool yaw = RotateYaw(targetPredictedPosition);

        if (cannon.State == CannonState.Ready && pitch && yaw)
        {
            SetState(AICannonControllerState.Shooting);
        }

#if UNITY_EDITOR
        if (isDebugMode)
        {
            float distance = Vector3.Distance(transform.position, target.transform.position) + (BOAT_LENGTH / 2);

            cannon.GetHitPrediction_IsDebugMode(target.transform.position.y);
            DebugUtil.DrawBox(targetPredictedPosition, target.transform.rotation, new Vector3(7, 7, BOAT_LENGTH), Color.red, Time.deltaTime);
        }
#endif
    }

    private Vector3 GetPredictedPosition()
    {
        float speed = Cannon.CANNONBALL_FORCE / Cannon.CANNONBALL_MASS;
        float distance = Vector3.Distance(transform.position, target.transform.position);
        float estimatedTime = distance / speed;

        Vector3 velocity = target.RigidBody.linearVelocity;
        velocity.y = 0;

        return target.transform.position + (velocity * estimatedTime);
    }

    private bool RotatePitch(Vector3 predictedPosition)
    {
        float actual = Vector3.Distance(transform.position, predictedPosition);
        float prediction = Vector3.Distance(transform.position, cannon.GetHitPrediction(predictedPosition.y));
        float difference = 1 - Mathf.Clamp(prediction / actual, 0, 2);
        cannon.ChangePitchTowards(difference);

        return Mathf.Abs(difference) < MAX_FIRE_PITCH_DIFFERENCE;
    }

    private bool RotateYaw(Vector3 predictedPosition)
    {
        cannon.ChangeYawTowards(Vector3.Cross((transform.position - predictedPosition).normalized, transform.forward).y);

        return GetCurrentAngle(predictedPosition) < GetAcceptableFireAngle(target, predictedPosition);
    }

    private float GetCurrentAngle(Vector3 position)
    {
        return Vector3.Angle(transform.forward, (position - transform.position).normalized);
    }

    private float GetAcceptableFireAngle(Boat _boat, Vector3 position)
    {
        Vector3 front = (GetOffsetPosition(_boat, position, BOAT_LENGTH / 2) - transform.position).normalized;
        Vector3 rear = (GetOffsetPosition(_boat, position, -BOAT_LENGTH / 2) - transform.position).normalized;

        return (Vector3.Angle(front, rear) / 2) + MAX_FIRE_YAW_DIFFERENCE;
    }

    private Vector3 GetOffsetPosition(Boat _boat, Vector3 position, float _offset)
    {
        return position + _boat.transform.TransformVector(new Vector3(0, 0, _offset));
    }

    private void FireAtTarget()
    {
        cannon.Fire();
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