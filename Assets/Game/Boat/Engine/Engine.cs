using UnityEngine;

public class Engine : MonoBehaviour
{
    private const float POWER = 100000;
    private const float ACCELERATION = 0.1f;
    private const float TURN_RADIUS = 3;
    private const float TURN_SPEEd = 0.3f;
    private const float STEERING_WHEEL_SPEED = 0.002f;
    private const float WHEELSPEED = 100;

    [Tooltip("The rotating paddlewheel")]
    [SerializeField] private Transform paddleWheel;

    private SteeringWheel steeringWheel;
    private Vector2 movement;
    private Rigidbody target;

#if UNITY_EDITOR
    [SerializeField] private bool isDebugMode;
#endif

    private void Awake()
    {
        target = GetComponentInParent<Rigidbody>();
        steeringWheel = transform.parent.GetComponentInChildren<SteeringWheel>();
    }

    private void FixedUpdate()
    {
        float x = Mathf.Clamp(movement.x * TURN_RADIUS, -TURN_RADIUS, TURN_RADIUS);
        transform.localPosition = new Vector3(x, transform.localPosition.y, transform.localPosition.z);

        float throttle = Mathf.Clamp(movement.y * POWER, 0, POWER);

        if (throttle > 0)
        {
            target.AddForceAtPosition(throttle * transform.forward, transform.position, ForceMode.Force);
            paddleWheel.Rotate(new Vector3(throttle * STEERING_WHEEL_SPEED * Time.deltaTime, 0, 0));
        }

#if UNITY_EDITOR
        if (isDebugMode)
        {
            Debug.DrawLine(transform.position + (Vector3.up * 4), transform.position + (Vector3.up * 4) - (transform.forward * movement.y * 5), Color.yellow, Time.fixedDeltaTime);
        }
#endif
    }

    public void ChangeMovement(Vector2 _movement)
    {
        if (_movement.x != 0)
        {
            float xPreLerp = movement.x;
            movement.x = Mathf.Clamp(Mathf.Lerp(movement.x, -_movement.x * 1.2f, TURN_SPEEd * Time.deltaTime), -1, 1);

            if (movement.x != xPreLerp)
            {
                steeringWheel.rotatingPart.transform.Rotate(new Vector3(0, 0, -_movement.x * WHEELSPEED * Time.deltaTime));
            }
        }

        if (_movement.y != 0)
        {
            movement.y = Mathf.Clamp(Mathf.Lerp(movement.y, _movement.y * 1.2f, ACCELERATION * Time.deltaTime), 0, 1);
        }
    }
}