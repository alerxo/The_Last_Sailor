using UnityEngine;

public class Engine : MonoBehaviour
{
    private InputSystem_Actions input;

    [SerializeField] private EnginePosition position;
    [SerializeField] private float acceleration, deceleration, maxSpeed, depth;
    private float speed;

    private Rigidbody target;

    private void Awake()
    {
        target = GetComponentInParent<Rigidbody>();

        input = new InputSystem_Actions();
        input.Player.Enable();
    }

    private void FixedUpdate()
    {
        Vector2 direction = input.Player.Move.ReadValue<Vector2>();

        bool isAcceleration = position == EnginePosition.Left ? direction.y > 0 || direction.x > 0 : direction.y > 0 || direction.x < 0;

        speed = Mathf.Clamp(speed + (isAcceleration ? acceleration : -deceleration), 0, maxSpeed);

        if (speed > 0) target.AddForceAtPosition(transform.forward * speed, transform.position, ForceMode.Acceleration);

        Debug.DrawLine(transform.position + transform.right, transform.position - (transform.forward * speed) + transform.right, Color.yellow, Time.fixedDeltaTime);
    }
}

public enum EnginePosition
{
    Left,
    Right
}