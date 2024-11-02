using UnityEngine;

public class Engine : MonoBehaviour
{
    private EngineState state = EngineState.Decelerating;

    [SerializeField] private float acceleration, deceleration, power;
    private float throttle;

    private Rigidbody target;

    private void Awake()
    {
        target = GetComponentInParent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        throttle = Mathf.Clamp(throttle + (state == EngineState.Decelerating ? -deceleration : acceleration), 0, 1);

        if (throttle > 0) target.AddForceAtPosition(power * throttle * transform.forward, transform.position, ForceMode.Force);

        Debug.DrawLine(transform.position + transform.right, transform.position - (transform.forward * throttle) + transform.right, Color.yellow, Time.fixedDeltaTime);
    }

    public void SetState(EngineState _state)
    {
        state = _state;
    }
}

public enum EngineState
{
    Decelerating,
    Accelerating
}