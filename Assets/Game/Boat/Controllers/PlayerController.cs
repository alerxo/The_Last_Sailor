using UnityEngine;

public class PlayerController : BoatController
{
    private InputSystem_Actions input;

    private void Awake()
    {
        input = new InputSystem_Actions();
        input.Player.LeftFire.performed += OnFireLeft;
        input.Player.RightFire.performed += OnFireRight;
        input.Player.Enable();
    }

    private void OnDestroy()
    {
        input.Player.LeftFire.performed -= OnFireLeft;
        input.Player.RightFire.performed -= OnFireRight;
    }

    private void Update()
    {
        Movement(input.Player.Move.ReadValue<Vector2>());
        ChangeCannonAngle(input.Player.CannonAngle.ReadValue<float>());
    }

    private void OnFireLeft(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        FireLeft();
    }

    private void OnFireRight(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        FireRight();
    }
}
