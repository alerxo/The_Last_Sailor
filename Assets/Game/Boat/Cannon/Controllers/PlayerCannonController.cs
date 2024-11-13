using UnityEngine;

public class PlayerCannonController : MonoBehaviour, IInteractable
{
    [Tooltip("Target for the cannon camera")]
    [SerializeField] private Transform cameraTarget;

    public Vector3 Position => transform.position;
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
        if (input.Player.Move.ReadValue<Vector2>().magnitude > 0)
        {
            Vector2 rotation = input.Player.Move.ReadValue<Vector2>();

            if (rotation.x != 0)
            {
                cannon.SetYaw(rotation.x);
            }

            if (rotation.y != 0)
            {
                cannon.SetPitch(rotation.y);
            }
        }
    }

    private void Fire_performed(UnityEngine.InputSystem.InputAction.CallbackContext _obj)
    {
        if (cannon.State == CannonState.Ready)
        {
            cannon.Fire();
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
        CameraManager.Instance.SetInteractionCamera(cameraTarget);
        FirstPersonController.instance.SetState(PlayerState.Cannon);
        input.Player.Enable();
    }
}
