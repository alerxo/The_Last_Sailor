using UnityEngine;

public class SteeringWheel : MonoBehaviour, IInteractable
{
    private const float MAX_ROTATION = 170;

    public Vector3 Position => transform.position;
    public bool CanInteract => true;

    [Tooltip("The rotating part of the mesh")]
    [SerializeField] private Transform rotatingPart;

    private InputSystem_Actions input;
    private Boat Boat;

    private void Awake()
    {
        Boat = GetComponentInParent<Boat>();

        input = new InputSystem_Actions();
        input.Player.ChangeCamera.performed += ChangeCamera_performed;

        FirstPersonController.OnPlayerStateChanged += FirstPersonController_OnPlayerStateChanged;
    }

    private void OnDestroy()
    {
        input.Player.Disable();
        input.Player.ChangeCamera.performed -= ChangeCamera_performed;
        FirstPersonController.OnPlayerStateChanged -= FirstPersonController_OnPlayerStateChanged;
    }

    private void Update()
    {
        if (input.Player.Move.ReadValue<Vector2>().x != 0)
        {
            Boat.Engine.ChangeRudder(input.Player.Move.ReadValue<Vector2>().x);
        }
    }

    public void SetRotation(float rotation)
    {
        rotatingPart.localRotation = Quaternion.Euler(new Vector3(0, 0, Mathf.Lerp(-MAX_ROTATION, MAX_ROTATION, (rotation + 1) / 2)));
    }

    public void Interact()
    {
        FirstPersonController.instance.SetState(PlayerState.SteeringWheel);
    }

    private void ChangeCamera_performed(UnityEngine.InputSystem.InputAction.CallbackContext _obj)
    {
        switch (CameraManager.Instance.State)
        {
            case CameraState.Player:
                CameraManager.Instance.SetState(CameraState.SteeringWheel);
                break;

            case CameraState.SteeringWheel:
                CameraManager.Instance.SetState(CameraState.Player);
                break;

            default:
                Debug.Log($"Defaulted with state: {CameraManager.Instance.State}");
                break;
        }
    }

    private void FirstPersonController_OnPlayerStateChanged(PlayerState _state)
    {
        if (_state == PlayerState.SteeringWheel) input.Player.Enable();
        else input.Player.Disable();
    }
}
