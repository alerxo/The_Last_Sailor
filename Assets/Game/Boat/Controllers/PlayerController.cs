using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private InputSystem_Actions input;
    [SerializeField] private Engine leftEngine, rightEngine;
    [SerializeField] private Cannon[] leftCannons, rightCannons;

    private void Awake()
    {
        input = new InputSystem_Actions();
        input.Player.LeftFire.performed += FireLeft;
        input.Player.RightFire.performed += FireRight;
        input.Player.Enable();
    }

    private void OnDestroy()
    {
        input.Player.LeftFire.performed -= FireLeft;
        input.Player.RightFire.performed -= FireRight;
    }

    private void Update()
    {
        Movement();
    }

    private void Movement()
    {
        Vector2 direction = input.Player.Move.ReadValue<Vector2>();

        if (direction.y > 0)
        {
            leftEngine.SetState(EngineState.Accelerating);
            rightEngine.SetState(EngineState.Accelerating);
        }

        else if (direction.x > 0)
        {
            leftEngine.SetState(EngineState.Accelerating);
            rightEngine.SetState(EngineState.Decelerating);
        }

        else if (direction.x < 0)
        {
            leftEngine.SetState(EngineState.Decelerating);
            rightEngine.SetState(EngineState.Accelerating);
        }

        else
        {
            leftEngine.SetState(EngineState.Decelerating);
            rightEngine.SetState(EngineState.Decelerating);
        }
    }

    private void FireLeft(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        Fire(leftCannons);
    }

    private void FireRight(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        Fire(rightCannons);
    }

    private void Fire(Cannon[] cannons)
    {
        foreach (Cannon cannon in cannons)
        {
            if (cannon.State == CannonState.Ready)
            {
                cannon.Fire();
            }
        }
    }
}
