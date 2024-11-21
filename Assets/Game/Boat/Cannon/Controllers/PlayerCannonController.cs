using UnityEngine;

public class PlayerCannonController : MonoBehaviour, IInteractable
{
    private const float ROTATION_SPEED = 1.6f;

    [Tooltip("Target for the cannon camera")]
    [SerializeField] private Transform cameraTarget;

    public Vector3 Position => transform.position;
    public bool CanInteract => cannon.State == CannonState.Ready;

    private InputSystem_Actions input;
    private Cannon cannon;

    private void Awake()
    {
        cannon = GetComponentInParent<Cannon>();

        input = new();
        input.Player.Fire.performed += Fire_performed;
        FirstPersonController.OnPlayerStateChanged += FirstPersonController_OnPlayerStateChanged;
    }

    private void OnDestroy()
    {
        input.Player.Disable();
        input.Player.Fire.performed -= Fire_performed;
        FirstPersonController.OnPlayerStateChanged -= FirstPersonController_OnPlayerStateChanged;
    }

    private void Update()
    {
        Vector2 rotation = input.Player.Look.ReadValue<Vector2>();

        if (rotation.magnitude == 0)
        {
            rotation = input.Player.Move.ReadValue<Vector2>();
        }

        if (rotation.magnitude > 0)
        {
            if (rotation.x != 0)
            {
                cannon.SetYaw(Mathf.Clamp(rotation.x, -ROTATION_SPEED, ROTATION_SPEED));
            }

            if (rotation.y != 0)
            {
                cannon.SetPitch(Mathf.Clamp(rotation.y, -ROTATION_SPEED, ROTATION_SPEED));
            }
        }
    }

    private void Fire_performed(UnityEngine.InputSystem.InputAction.CallbackContext _obj)
    {
        if (cannon.State == CannonState.Ready)
        {
            cannon.Fire();
            CameraManager.Instance.SetState(CameraState.Player);
            FirstPersonController.Instance.SetState(PlayerState.FirstPerson);
        }
    }

    private void FirstPersonController_OnPlayerStateChanged(PlayerState _state)
    {
        if (_state != PlayerState.Cannon)
        {
            input.Player.Disable();
        }
    }

    public void Interact()
    {
        if (cannon.State == CannonState.Ready)
        {
            CameraManager.Instance.SetInteractionCamera(cameraTarget);
            FirstPersonController.Instance.SetState(PlayerState.Cannon);
            input.Player.Enable();
        }
    }
}
