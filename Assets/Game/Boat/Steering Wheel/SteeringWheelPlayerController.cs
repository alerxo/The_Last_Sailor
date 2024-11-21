using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.Splines;

public class SteeringWheelPlayerController : MonoBehaviour, IInteractable
{
    private const float FORCE_PLAYER_POSITION_DURATION = 0.1f;
    private const float FORCE_STRAIGHTEN_UP_MARGIN = 0.1f;

    public Vector3 Position => transform.position + transform.TransformVector(new(0, 0, -1.5f));
    public bool CanInteract => true;
    public Transform Transform => transform;

    private InputSystem_Actions input;
    private Boat Boat;
    private FirstPersonController player;
    private CinemachineCamera playerCamera;

    private void Awake()
    {
        Boat = GetComponentInParent<Boat>();
        player = FindFirstObjectByType<FirstPersonController>();

        input = new InputSystem_Actions();
        input.Player.ChangeCamera.performed += ChangeCamera_performed;

        FirstPersonController.OnPlayerStateChanged += FirstPersonController_OnPlayerStateChanged;
    }

    private void Start()
    {
        playerCamera = CameraManager.Instance.PlayerCamera;
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

        else if (Mathf.Abs(Boat.Engine.Rudder) < FORCE_STRAIGHTEN_UP_MARGIN)
        {
            Boat.Engine.ChangeTowardsRudder(0);
        }
    }

    public void Interact()
    {
        FirstPersonController.Instance.SetState(PlayerState.SteeringWheel);
    }

    private void ChangeCamera_performed(UnityEngine.InputSystem.InputAction.CallbackContext _obj)
    {
        switch (CameraManager.Instance.State)
        {
            case CameraState.Player:
                playerCamera.ForceCameraPosition(playerCamera.transform.position, transform.rotation);
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
        if (_state == PlayerState.SteeringWheel)
        {
            input.Player.Enable();
            StartCoroutine(ForcePlayerAtPosition());
        }

        else
        {
            input.Player.Disable();
        }
    }

    private IEnumerator ForcePlayerAtPosition()
    {
        float duration = 0;
        player.transform.GetPositionAndRotation(out Vector3 startPosition, out Quaternion startRotation);

        Vector3 nextPosition;
        Quaternion nextRotation;

        while (input.Player.enabled && (duration += Time.deltaTime) < FORCE_PLAYER_POSITION_DURATION)
        {
            float percentage = duration / FORCE_PLAYER_POSITION_DURATION;

            nextPosition = Vector3.Lerp(startPosition, Position, percentage);
            nextPosition.y = player.transform.position.y;
            nextRotation = Quaternion.Lerp(startRotation, transform.rotation, percentage);

            playerCamera.ForceCameraPosition(playerCamera.transform.position, nextRotation);
            player.Rigidbody.Move(nextPosition, playerCamera.transform.rotation);

            yield return null;
        }

        nextPosition = Position;
        nextPosition.y = player.transform.position.y;
        nextRotation = transform.rotation;

        playerCamera.ForceCameraPosition(playerCamera.transform.position, nextRotation);
        player.Rigidbody.Move(nextPosition, playerCamera.transform.rotation);
    }
}
