using UnityEngine;

public class Engine : MonoBehaviour
{
    private const float POWER = 100000f;
    private const float ACCELERATION = 1f;
    private const float TURN_RADIUS = 3f;
    private const float TURN_SPEEd = 1f;
    private const float PADDLE_WHEEL_SPEED = 0.002f;

    [Tooltip("The rotating paddlewheel")]
    [SerializeField] private Transform paddleWheel;

    private SteeringWheel steeringWheel;
    private Throttle throttle;
    private Vector2 movement;
    private Rigidbody target;

#if UNITY_EDITOR
    [SerializeField] private bool isDebugMode;
#endif

    private void Awake()
    {
        target = GetComponentInParent<Rigidbody>();
        steeringWheel = transform.parent.GetComponentInChildren<SteeringWheel>();
        throttle = transform.parent.GetComponentInChildren<Throttle>();
    }

    private void FixedUpdate()
    {
        float x = Mathf.Clamp(movement.x * TURN_RADIUS, -TURN_RADIUS, TURN_RADIUS);
        transform.localPosition = new Vector3(x, transform.localPosition.y, transform.localPosition.z);

        float throttle = Mathf.Clamp(movement.y * POWER, 0, POWER);

        if (throttle > 0)
        {
            target.AddForceAtPosition(throttle * transform.forward, transform.position, ForceMode.Force);
            paddleWheel.Rotate(new Vector3(throttle * PADDLE_WHEEL_SPEED * Time.deltaTime, 0, 0));
        }

#if UNITY_EDITOR
        if (isDebugMode)
        {
            Debug.DrawLine(transform.position + (Vector3.up * 4), transform.position + (Vector3.up * 4) - (transform.forward * movement.y * 5), Color.yellow, Time.fixedDeltaTime);
        }
#endif
    }

    public void ChangeRudder(float _rudder)
    {
        movement.x = Mathf.Clamp(Mathf.Lerp(movement.x, -_rudder * 1.2f, TURN_SPEEd * Time.deltaTime), -1, 1);
        steeringWheel.SetRotation(movement.x);
    }

    public void ChangeThrottle(float _throttle)
    {
        movement.y = Mathf.Clamp(Mathf.Lerp(movement.y, _throttle * 1.2f, ACCELERATION * Time.deltaTime), 0, 1);
        throttle.SetRotation(movement.y);
    }
}