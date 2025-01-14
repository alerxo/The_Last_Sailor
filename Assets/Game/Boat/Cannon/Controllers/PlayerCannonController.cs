using System.Collections;
using UnityEngine;

public class PlayerCannonController : MonoBehaviour, IInteractable
{
    private const float ROTATION_SPEED = 1f;
    private const float MOUSE_SPEED = 0.7f;

    [Tooltip("Target for the cannon camera")]
    [SerializeField] private Transform cameraTarget;

    public Vector3 Position => transform.position + transform.TransformVector(new(0, 0, -2));
    public bool CanInteract => cannon.State == CannonState.Ready;
    public Transform Transform => transform;
    public Renderer[] GetRenderers => renderers;
    private Renderer[] renderers;

    private InputSystem_Actions input;
    private Cannon cannon;

    private void Awake()
    {
        cannon = GetComponent<Cannon>();

        renderers = cannon.GetComponentsInChildren<Renderer>(true);

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
        Vector2 keyboardMovement = input.Player.Move.ReadValue<Vector2>();
        Vector2 mouseMovement = input.Player.Look.ReadValue<Vector2>() / 100f * MOUSE_SPEED * MouseSensitivityManager.Instance.CannonMouseSensitivity;
        Vector2 movement = keyboardMovement.magnitude != 0 ? keyboardMovement : mouseMovement;

        if (movement.x != 0)
        {
            cannon.ChangeYaw(movement.x * ROTATION_SPEED);
        }

        if (movement.y != 0)
        {
            cannon.ChangePitch(movement.y * ROTATION_SPEED);
        }
    }

    private void CannonFire_performed(UnityEngine.InputSystem.InputAction.CallbackContext _obj)
    {
        if (cannon.State == CannonState.Ready)
        {
            HUDScreen.Instance.CompleteObjective(ObjectiveType.ShootCannon);
            cannon.Fire(CannonballOwner.Player);
            CameraManager.Instance.SetState(CameraState.Player);
            FirstPersonController.Instance.SetState(PlayerState.FirstPerson);
        }
    }

    private void FirstPersonController_OnPlayerStateChanged(PlayerState _state)
    {
        if (_state != PlayerState.Cannon)
        {
            input.Player.Disable();
            TutorialScreen.Instance.HideTutorial(TutorialType.Cannon);
        }
    }

    public void Interact()
    {
        if (cannon.State == CannonState.Ready)
        {
            CameraManager.Instance.SetInteractionCamera(cameraTarget, this);
            FirstPersonController.Instance.SetState(PlayerState.Cannon);
            input.Player.Enable();
            TutorialScreen.Instance.ShowInputTooltip(TutorialType.Cannon);
        }
    }
}
