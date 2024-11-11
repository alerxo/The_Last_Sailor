using UnityEngine;

public class Engine : MonoBehaviour
{
    private Vector2 movement;

    [SerializeField] private float power;
    [SerializeField] private float acceleration;
    [SerializeField] private float turnRadius;
    [SerializeField] private float turnSpeed;

    private Rigidbody target;

#if UNITY_EDITOR
    [SerializeField] private bool isDebugMode;
#endif

    private void Awake()
    {
        target = GetComponentInParent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        float x = Mathf.Clamp(movement.x * turnRadius, -turnRadius, turnRadius);
        transform.localPosition = new Vector3(x, transform.localPosition.y, transform.localPosition.z);

        float throttle = Mathf.Clamp(movement.y * power, 0, power);

        if (throttle > 0)
        {
            target.AddForceAtPosition(throttle * transform.forward, transform.position, ForceMode.Force);
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
        if (_movement.x != 0) movement.x = Mathf.Lerp(movement.x, -_movement.x, turnSpeed * Time.deltaTime);
        if (_movement.y != 0) movement.y = Mathf.Lerp(movement.y, _movement.y, acceleration * Time.deltaTime);
    }
}