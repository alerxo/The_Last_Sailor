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

        UIManager.OnStateChanged += UIManager_OnStateChanged;
    }

    private void OnDestroy()
    {
        input.Player.LeftFire.performed -= OnFireLeft;
        input.Player.RightFire.performed -= OnFireRight;

        UIManager.OnStateChanged -= UIManager_OnStateChanged;
    }

    protected override void OnHit()
    {
        base.OnHit();

        CameraManager.Instance.ShakeCamera(5, 0.3f);
    }

    private void UIManager_OnStateChanged(UIState _state)
    {
        if (_state == UIState.Game) input.Player.Enable();
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
