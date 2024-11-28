using UnityEngine;

public class Engine : MonoBehaviour, IUpgradeable
{
    private const float POWER = 100000f;
    private const float THROTTLE_ACCELERATION = 0.9f;
    private const float RUDDER_ACCELERATION = 0.9f;
    private const float TURN_RADIUS = 3f;
    private const float RUDDER_PASSIVE_TORQUE = 20000f;
    private const float PADDLE_WHEEL_SPEED = 0.002f;

    [Tooltip("The rotating paddlewheel")]
    [SerializeField] private Transform paddleWheel;
    [SerializeField] private AudioSource paddleAudioSource;
    [SerializeField] private AudioClip paddleAudioClip;

    private SteeringWheel steeringWheel;
    private Throttle throttle;
    public float Rudder { get; private set; }
    public float Throttle { get; private set; }

    public UpgradeTier UpgradeTier { get; set; }

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
        float rudder = Mathf.Clamp(Rudder * TURN_RADIUS, -TURN_RADIUS, TURN_RADIUS);
        transform.localPosition = new Vector3(rudder, transform.localPosition.y, transform.localPosition.z);

        if (rudder != 0)
        {
            target.AddTorque(0, -rudder * RUDDER_PASSIVE_TORQUE, 0, ForceMode.Force);
        }

        float throttle = Mathf.Clamp(Throttle * POWER, 0, POWER);

        if (throttle > 0)
        {
            target.AddForceAtPosition(throttle * transform.forward, transform.position, ForceMode.Force);
            paddleWheel.Rotate(new Vector3(throttle * PADDLE_WHEEL_SPEED * Time.deltaTime, 0, 0));

            TryPlayerPaddleWheelAudio();
        }

        else
        {
            StopPaddleWheelAudio();
        }

#if UNITY_EDITOR
        if (isDebugMode)
        {
            Debug.DrawLine(transform.position + (Vector3.up * 4), transform.position + (Vector3.up * 4) - (transform.forward * Rudder * 5), Color.yellow, Time.fixedDeltaTime);
        }
#endif
    }

    private void TryPlayerPaddleWheelAudio()
    {
        if (paddleAudioSource.isPlaying == false)
        {
            paddleAudioSource.Play();
        }

        paddleAudioSource.volume = Throttle / 2;
        paddleAudioSource.pitch = Throttle;
    }

    private void StopPaddleWheelAudio()
    {
        paddleAudioSource.Stop();
        paddleAudioSource.volume = 0;
    }

    public void ChangeRudder(float _rudder)
    {
        Rudder = Mathf.Clamp(Rudder + (-_rudder * RUDDER_ACCELERATION * Time.deltaTime), -1, 1);
        steeringWheel.SetRotation(Rudder);
    }

    public void ChangeTowardsRudder(float _rudder)
    {
        Rudder = Mathf.Clamp(Mathf.Lerp(Rudder, -_rudder, RUDDER_ACCELERATION * Time.deltaTime), -1, 1);
        steeringWheel.SetRotation(Rudder);
    }

    public void ChangeThrottle(float _throttle)
    {
        Throttle = Mathf.Clamp01(Throttle + (_throttle * THROTTLE_ACCELERATION * Time.deltaTime));
        throttle.SetRotation(Throttle);
    }

    public void ChangeTowardsThrottle(float _throttle)
    {
        Throttle = Mathf.Clamp(Mathf.Lerp(Throttle, _throttle, THROTTLE_ACCELERATION * Time.deltaTime), 0, 1);
        throttle.SetRotation(Throttle);
    }

    public float GetUpgradeValue()
    {
        switch (UpgradeTier)
        {
            case UpgradeTier.First:
                return POWER;

            case UpgradeTier.Second:
                return POWER * 1.5f;

            case UpgradeTier.Third:
                return POWER * 2f;

            default:
                Debug.LogError("Defaulted in GetUpgradeTier");
                return 0;
        }
    }
}