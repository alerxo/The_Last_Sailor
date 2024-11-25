using UnityEngine;

public class AICannonController : MonoBehaviour
{
    private const float MAX_TARGET_ANGLE = 100f;
    private const float MAX_FIRE_YAW_DIFFERENCE = 7f;
    private const float BOAT_LENGTH = 15f;
    private const float CANNONBALL_DRAG_ESTIMATION = 1.03f;

    private AICannonControllerState state;

    private AIBoatController owner;
    private Cannon cannon;
    private Boat target;

    Vector3 targetPredictedPosition;
    float predictionDistance;
    private float predictionTimer;
    private const float PREDICTION_COOLDOWN = 0.2f;

#if UNITY_EDITOR
    [SerializeField] private bool isDebugMode;
#endif

    private void Awake()
    {
        predictionTimer = Random.Range(0, PREDICTION_COOLDOWN);

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
            TryUpdatePredictions();
            AimAtTarget();
        }

        if (state == AICannonControllerState.Shooting)
        {
            FireAtTarget();
        }
    }

    private void CheckIfActive()
    {
        if (owner.Admiral == null || owner.Admiral.Enemy == null || owner.Boat.Health <= 0)
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

    private void TryUpdatePredictions()
    {
        if ((predictionTimer += Time.deltaTime) > PREDICTION_COOLDOWN)
        {
            predictionTimer = 0f;
            CalculatePredictions();
        }
    }

    private void CalculatePredictions()
    {
        float speed = Cannon.CANNONBALL_FORCE / Cannon.CANNONBALL_MASS;
        float distance = Vector3.Distance(transform.position, target.transform.position);
        float estimatedTime = distance / speed * CANNONBALL_DRAG_ESTIMATION;

        Vector3 velocity = target.RigidBody.linearVelocity;
        velocity.y = 0;

        targetPredictedPosition = target.transform.position + (velocity * estimatedTime);
        predictionDistance = Vector3.Distance(transform.position, cannon.GetHitPrediction(targetPredictedPosition.y));
    }

    private void AimAtTarget()
    {
        RotatePitch(targetPredictedPosition);
        bool hasAcceptableYaw = RotateYaw(targetPredictedPosition);

        if (cannon.State == CannonState.Ready && hasAcceptableYaw)
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

    private void RotatePitch(Vector3 predictedPosition)
    {
        float actual = Vector3.Distance(transform.position, predictedPosition);
        float difference = 1 - Mathf.Clamp(predictionDistance / actual, 0, 2);
        cannon.ChangePitchTowards(difference);
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