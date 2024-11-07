using UnityEngine;

public class PlayerCannonController : MonoBehaviour, IInteractable
{
    public Vector3 Position => transform.position;

    private InputSystem_Actions input;

    [SerializeField] private Transform cameraTarget;

    private Cannon cannon;

    private void Awake()
    {
        cannon = GetComponent<Cannon>();

        input = new();
        input.Player.Fire.performed += Fire_performed;
        CameraManager.OnStateChanged += CameraManager_OnStateChanged;
    }

    private void OnDestroy()
    {
        input.Player.Fire.performed -= Fire_performed;
        CameraManager.OnStateChanged -= CameraManager_OnStateChanged;
    }

    private void Update()
    {
        if (input.Player.Move.ReadValue<Vector2>().magnitude > 0)
        {
            cannon.Rotate(input.Player.Move.ReadValue<Vector2>());
            CameraManager.Instance.SetCannonCameraPosition(cameraTarget);
        }
    }

    private void Fire_performed(UnityEngine.InputSystem.InputAction.CallbackContext _obj)
    {
        if (cannon.State == CannonState.Ready)
        {
            cannon.Fire();
        }
    }

    private void CameraManager_OnStateChanged(CameraState _state)
    {
        input.Player.Disable();
    }

    public void Interact()
    {
        CameraManager.Instance.SetCannonCamera(cameraTarget);
        input.Player.Enable();
    }
}
