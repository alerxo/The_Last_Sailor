using UnityEngine;

public class ThrottlePlayerController : MonoBehaviour, IInteractable
{
    public Vector3 Position => transform.position + transform.TransformVector(new(0, 0, 2));
    public bool CanInteract => true;
    public Transform Transform => transform;

    [SerializeField] private Transform cameraTarget;

    private InputSystem_Actions input;
    private Boat Boat;

    private void Awake()
    {
        Boat = GetComponentInParent<Boat>();

        input = new InputSystem_Actions();

        FirstPersonController.OnPlayerStateChanged += FirstPersonController_OnPlayerStateChanged;

        GetComponentInParent<Throttle>().SetRotation(0);
    }

    private void OnDestroy()
    {
        input.Player.Disable();
        FirstPersonController.OnPlayerStateChanged -= FirstPersonController_OnPlayerStateChanged;
    }

    private void Update()
    {
        if (input.Player.Move.ReadValue<Vector2>().x != 0)
        {
            Boat.Engine.ChangeThrottle(input.Player.Move.ReadValue<Vector2>().x);
        }
    }

    public void Interact()
    {
        CameraManager.Instance.SetInteractionCamera(cameraTarget, this);
        FirstPersonController.Instance.SetState(PlayerState.Throttle);
    }

    private void FirstPersonController_OnPlayerStateChanged(PlayerState _state)
    {
        if (_state == PlayerState.Throttle) input.Player.Enable();
        else input.Player.Disable();
    }
}
