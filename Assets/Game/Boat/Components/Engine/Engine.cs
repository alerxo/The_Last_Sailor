using UnityEngine;

public class Engine : MonoBehaviour
{
    private Vector2 movement;

    [SerializeField] private float power;
    [SerializeField] private float acceleration;
    [SerializeField] private float turnRadius;
    [SerializeField] private float turnSpeed;

    private Rigidbody target;

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

        Debug.DrawLine(transform.position, transform.position - (transform.forward * movement.y * 5), Color.yellow, Time.fixedDeltaTime);
    }

    public void ChangeMovement(Vector2 _movement)
    {
        movement.x = Mathf.Lerp(movement.x, -_movement.x, turnSpeed * Time.deltaTime);
        movement.y = Mathf.Lerp(movement.y, _movement.y, acceleration * Time.deltaTime);
    }
}