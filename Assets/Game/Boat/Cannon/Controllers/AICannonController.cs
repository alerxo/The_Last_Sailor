using UnityEngine;

public class AICannonController : MonoBehaviour
{
    private const float MAX_TARGET_ANGLE = 30f;
    private const float CANNONBALL_DRAG_ESTIMATION = 1.03f;

    private AICannonControllerState state;

    private AIBoatController owner;
    private Cannon cannon;
    [SerializeField] private Transform cannonTransform;

    private Boat target;
    private float targetingTimer;
    private const float TARGETING_COOLDOWN = 1f;

    private CannonPrediction predictedTrajectory;
    private Vector3 predictedTargetPosition;
    private float predictionTimer;
    private const float PREDICTION_COOLDOWN = 0.1f;

    private void Awake()
    {
        targetingTimer = Random.Range(0, TARGETING_COOLDOWN);
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
            CheckIfValidTarget();
        }

        if (state == AICannonControllerState.Aiming)
        {
            TryUpdatePredictions();
            RotatePitch();
            RotateYaw();
            CheckIfCanFire();

#if UNITY_EDITOR
            if (owner.IsDebugMode && target != null)
            {
                cannon.DebugDrawTrajectory(target, predictedTargetPosition);
            }
#endif
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
        if ((targetingTimer += Time.deltaTime) < TARGETING_COOLDOWN) return;

        targetingTimer = 0f;

        float closest = float.MaxValue;
        target = null;

        foreach (Boat boat in owner.Admiral.Enemy.Fleet)
        {
            float distance = Vector3.Distance(cannonTransform.position, boat.transform.position);

            if (distance < closest && IsValidTarget(boat, distance))
            {
                target = boat;
                closest = distance;
            }
        }
    }

    private void CheckIfValidTarget()
    {
        if (target != null && !target.IsSunk)
        {
            SetState(AICannonControllerState.Aiming);
        }
    }

    private bool IsValidTarget(Boat _boat, float _distance)
    {
        return !_boat.IsSunk && _distance < CombatManager.RING_OF_FIRE_SIZE && GetCurrentAngle(_boat.transform.position) <= MAX_TARGET_ANGLE;
    }

    private float GetCurrentAngle(Vector3 position)
    {
        return Vector3.Angle(cannonTransform.forward, (position - cannonTransform.position).normalized);
    }

    private void TryUpdatePredictions()
    {
        if ((predictionTimer += Time.deltaTime) < PREDICTION_COOLDOWN) return;

        predictionTimer = 0f;

        float speed = Cannon.CANNONBALL_FORCE / Cannon.CANNONBALL_MASS;
        float distance = Vector3.Distance(cannonTransform.position, target.transform.position);
        float estimatedTime = distance / speed * CANNONBALL_DRAG_ESTIMATION;

        predictedTrajectory = cannon.GetHitPrediction(target, predictedTargetPosition);
        predictedTargetPosition = target.transform.position + (target.RigidBody.linearVelocity * estimatedTime);
    }

    private void RotatePitch()
    {
        if (IsPredictedHitEnemy())
        {
            float difference = predictedTargetPosition.y - predictedTrajectory.Point.y;
            cannon.ChangePitch(difference);
        }

        else
        {
            float distance = Vector3.Distance(cannonTransform.position, predictedTargetPosition);
            float difference = 1 - Mathf.Clamp(Vector3.Distance(cannonTransform.position, predictedTrajectory.Point) / distance, 0, 2);
            cannon.ChangePitch(difference);
        }
    }

    public float cross;

    private void RotateYaw()
    {
        cross = Vector3.Cross((cannonTransform.position - predictedTargetPosition).normalized, cannonTransform.forward).y;

        cannon.ChangeYaw(cross);
    }

    private void CheckIfCanFire()
    {
        if (cannon.State == CannonState.Ready && IsPredictedHitEnemy())
        {
            SetState(AICannonControllerState.Shooting);
        }
    }

    private bool IsPredictedHitEnemy()
    {
        if (predictedTrajectory.Hit == null) return false;

        foreach (Boat boat in owner.Admiral.Fleet)
        {
            if (predictedTrajectory.Hit == boat)
            {
                return false;
            }
        }

        return true;
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