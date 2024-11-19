using System.Collections;
using UnityEngine;

public class SteeringWheelPlayerController : MonoBehaviour, IInteractable
{
    private const float FORCE_PLAYER_POSITION_DURATION = 0.2f;
    private static readonly Vector3 playerPosition = new(0, 0, -1);

    public Vector3 Position => transform.position;
    public bool CanInteract => true;

    private InputSystem_Actions input;
    private Boat Boat;
    private FirstPersonController player;

    private void Awake()
    {
        Boat = GetComponentInParent<Boat>();
        player = FindFirstObjectByType<FirstPersonController>();

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
        Vector3 startPosition = player.transform.position;
        Quaternion startRotation = Quaternion.identity;

        while (input.Player.enabled && (duration += Time.deltaTime) < FORCE_PLAYER_POSITION_DURATION)
        {
            float percentage = duration / FORCE_PLAYER_POSITION_DURATION;
            player.GetComponent<Rigidbody>().Move(
                Vector3.Lerp(startPosition, transform.position + playerPosition, percentage),
                Quaternion.Lerp(startRotation, transform.rotation, percentage));

            yield return null;
        }

        player.GetComponent<Rigidbody>().Move(transform.position + playerPosition, transform.rotation);
    }
}
