using NUnit.Framework.Internal.Commands;
using UnityEngine;

public class ThrottlePlayerController : MonoBehaviour, IInteractable
{
    public Vector3 Position => transform.position + transform.TransformVector(new(0, 0, 2));
    public bool CanInteract => true;
    public Transform Transform => transform;

    [SerializeField] private Transform cameraTarget;
    [SerializeField] private AudioSource throttleAudioSource;
    [SerializeField] private AudioClip throttleAudioClip;
    [SerializeField] private AudioSource stopThrottleAudioSource;
    [SerializeField] private AudioClip stopThrottleAudioClip;

    private InputSystem_Actions input;
    private Boat Boat;
    bool allowThrottleSqueekPlayed;
    bool allowStopTurningSoundPlayed;
    bool skipfirstsqueek;

    private void Awake()
    {
        Boat = GetComponentInParent<Boat>();

        input = new InputSystem_Actions();

        FirstPersonController.OnPlayerStateChanged += FirstPersonController_OnPlayerStateChanged;

        GetComponentInParent<Throttle>().SetRotation(0);

        throttleAudioSource.clip = throttleAudioClip;
        stopThrottleAudioSource.clip = stopThrottleAudioClip;

        allowStopTurningSoundPlayed = false;
        allowThrottleSqueekPlayed = true;
        skipfirstsqueek = false;
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
            if (!throttleAudioSource.isPlaying && allowThrottleSqueekPlayed) 
            {
                throttleAudioSource.volume = 0.1f;
                throttleAudioSource.pitch = Random.Range(0.7f, 0.8f);
                throttleAudioSource.Play();
            }
        }
        else
        {
            throttleAudioSource.volume = throttleAudioSource.volume - 0.02f;
            if (throttleAudioSource.volume == 0)
            {
                throttleAudioSource.Stop();
            }
        }
        if (Boat.Engine.Throttle >= 1f || Boat.Engine.Throttle <= 0f)
        {
            if (!stopThrottleAudioSource.isPlaying && allowStopTurningSoundPlayed == true)
            {
                allowStopTurningSoundPlayed = false;
                allowThrottleSqueekPlayed = false;
                throttleAudioSource.Stop();
                if (skipfirstsqueek == true)
                {
                    stopThrottleAudioSource.Play();
                }
            }
        }
        if (Boat.Engine.Throttle < 1f && Boat.Engine.Throttle > 0f)
        {
            allowThrottleSqueekPlayed = true;
            allowStopTurningSoundPlayed = true;
            skipfirstsqueek = true;
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
