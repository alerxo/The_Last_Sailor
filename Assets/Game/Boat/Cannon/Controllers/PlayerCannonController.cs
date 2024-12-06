using System.Runtime.CompilerServices;
using UnityEngine;

public class PlayerCannonController : MonoBehaviour, IInteractable
{
    private const float ROTATION_SPEED = 1.6f;

    [Tooltip("Target for the cannon camera")]
    [SerializeField] private Transform cameraTarget;

    public Vector3 Position => transform.position + transform.TransformVector(new(0, 0, -2));
    public bool CanInteract => cannon.State == CannonState.Ready;
    public Transform Transform => transform;

    private InputSystem_Actions input;
    private Cannon cannon;

    private void Awake()
    {
        cannon = GetComponentInParent<Cannon>();

        input = new();
        input.Player.CannonFire.performed += CannonFire_performed;
        FirstPersonController.OnPlayerStateChanged += FirstPersonController_OnPlayerStateChanged;
    }

    private void OnDestroy()
    {
        input.Player.Disable();
        input.Player.CannonFire.performed -= CannonFire_performed;
        FirstPersonController.OnPlayerStateChanged -= FirstPersonController_OnPlayerStateChanged;
    }

    private void Update()
    {
        Vector2 rotation = input.Player.Move.ReadValue<Vector2>();

        if (rotation.magnitude == 0)
        {
            rotation = input.Player.Look.ReadValue<Vector2>();
            rotation.x = Mathf.Clamp(rotation.x, -ROTATION_SPEED, ROTATION_SPEED) / 1.5f;
            rotation.y = Mathf.Clamp(rotation.y, -ROTATION_SPEED, ROTATION_SPEED) / 1.5f;
        }

        if (rotation.magnitude > 0)
        {
            if (rotation.x != 0)
            {
                cannon.ChangeYaw(Mathf.Clamp(rotation.x, -ROTATION_SPEED, ROTATION_SPEED));
            }

            if (rotation.y != 0)
            {
                cannon.ChangePitch(Mathf.Clamp(rotation.y, -ROTATION_SPEED, ROTATION_SPEED));
            }
        }
    }

    private void CannonFire_performed(UnityEngine.InputSystem.InputAction.CallbackContext _obj)
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
            CameraManager.Instance.SetInteractionCamera(cameraTarget, this);
            FirstPersonController.Instance.SetState(PlayerState.Cannon);
            input.Player.Enable();
        }
    }
}
