using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class Engine : MonoBehaviour
{
    [SerializeField] private float acceleration, deceleration, maxSpeed, depth;
    private float leftSpeed, rightSpeed;

    private InputSystem_Actions input;

    [SerializeField] private Transform left, right;
    private Rigidbody target;

    private void Awake()
    {
        target = GetComponent<Rigidbody>();

        input = new InputSystem_Actions();
        input.Player.Enable();
    }

    private void FixedUpdate()
    {
        Vector2 direction = input.Player.Move.ReadValue<Vector2>();

        leftSpeed = Mathf.Clamp(leftSpeed + ((direction.y > 0 || direction.x > 0) ? acceleration : -deceleration), 0, maxSpeed);
        rightSpeed = Mathf.Clamp(rightSpeed + ((direction.y > 0 || direction.x < 0) ? acceleration : -deceleration), 0, maxSpeed);

        if (leftSpeed > 0)
        {
            target.AddForceAtPosition(transform.forward * leftSpeed, left.position, ForceMode.Acceleration);
        }

        if (rightSpeed > 0)
        {
            target.AddForceAtPosition(transform.forward * rightSpeed, right.position, ForceMode.Acceleration);
        }

        Debug.DrawLine(left.position - transform.right, left.position - (transform.forward * leftSpeed) - transform.right, Color.yellow, Time.fixedDeltaTime);
        Debug.DrawLine(right.position + transform.right, right.position - (transform.forward * rightSpeed) + transform.right, Color.yellow, Time.fixedDeltaTime);
    }
}