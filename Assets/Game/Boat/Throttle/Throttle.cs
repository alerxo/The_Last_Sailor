using UnityEngine;

public class Throttle : MonoBehaviour, IInteractable
{
    private const float MAX_ROTATION = 30;

    public Vector3 Position => transform.position;

    [Tooltip("The rotating part of the mesh")]
    [SerializeField] private Transform rotatingPart;
    [SerializeField] private Transform cameraTarget;

    private InputSystem_Actions input;
    private Boat Boat;

    private void Awake()
    {
        Boat = GetComponentInParent<Boat>();

        input = new InputSystem_Actions();

        FirstPersonController.OnPlayerStateChanged += FirstPersonController_OnPlayerStateChanged;

        SetRotation(0);
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

    public void SetRotation(float rotation)
    {
        rotatingPart.localRotation = Quaternion.Euler(new Vector3(0, 0, Mathf.Lerp(-MAX_ROTATION, MAX_ROTATION, (rotation + 1) / 2) - MAX_ROTATION / 2));
    }

    public void Interact()
    {
        CameraManager.Instance.SetInteractionCamera(cameraTarget);
        FirstPersonController.instance.SetState(PlayerState.Throttle);
    }

    private void FirstPersonController_OnPlayerStateChanged(PlayerState _state)
    {
        if (_state == PlayerState.Throttle) input.Player.Enable();
        else input.Player.Disable();
    }
}
