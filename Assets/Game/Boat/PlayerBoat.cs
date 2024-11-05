using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerBoat : Boat
{
    private InputSystem_Actions input;

    private void Awake()
    {
        input = new InputSystem_Actions();
        input.Player.LeftFire.performed += OnFireLeft;
        input.Player.RightFire.performed += OnFireRight;

        CameraManager.OnStateChanged += CameraManager_OnStateChanged;
    }

    private void OnDestroy()
    {
        input.Player.LeftFire.performed -= OnFireLeft;
        input.Player.RightFire.performed -= OnFireRight;

        CameraManager.OnStateChanged -= CameraManager_OnStateChanged;
    }

    protected override void OnHit()
    {
        base.OnHit();

        CameraManager.Instance.ShakeCamera(3, 0.6f);
    }

    private void CameraManager_OnStateChanged(CameraState _state)
    {
        if (_state == CameraState.Boat) input.Player.Enable();
        else input.Player.Disable();
    }

    public override void Destroyed()
    {
        SceneManager.LoadScene("Game");
    }

    private void Update()
    {
        Movement(input.Player.Move.ReadValue<Vector2>());
        ChangeCannonAngle(input.Player.CannonAngle.ReadValue<float>());
    }

    private void OnFireLeft(UnityEngine.InputSystem.InputAction.CallbackContext _obj)
    {
        FireLeft();
    }

    private void OnFireRight(UnityEngine.InputSystem.InputAction.CallbackContext _obj)
    {
        FireRight();
    }
}
