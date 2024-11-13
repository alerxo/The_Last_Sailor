using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerBoatController : Boat
{
    protected override float MaxHealth => 100;

    private InputSystem_Actions input;

    public override void Awake()
    {
        base.Awake();

        input = new InputSystem_Actions();

        CameraManager.OnStateChanged += CameraManager_OnStateChanged;
    }

    private void OnDestroy()
    {
        input.Player.Disable();
        CameraManager.OnStateChanged -= CameraManager_OnStateChanged;
    }

    protected override void OnHit()
    {
        base.OnHit();

        CameraManager.Instance.ShakeCamera(1f, 0.7f);
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
        ChangeMovement(input.Player.Move.ReadValue<Vector2>());
        
    }
}
