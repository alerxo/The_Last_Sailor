using System.Collections.Generic;
using UnityEngine;

public class ThrottlePlayerController : MonoBehaviour, IInteractable
{
    public Vector3 Position => transform.position + transform.TransformVector(new(0, 0, 2));
    public bool CanInteract => true;
    public Transform Transform => transform;
    public Renderer[] GetRenderers => renderers;
    private Renderer[] renderers;
    [SerializeField] private Transform[] throttleMeshes;

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
        List<Renderer> list = new();

        foreach (Transform t in throttleMeshes)
        {
            list.AddRange(t.GetComponentsInChildren<Renderer>());
        }

        foreach(Renderer renderer in GetComponentInParent<Throttle>().GetComponentsInChildren<Renderer>(true))
        {
            list.Add(renderer);
        }

        renderers = list.ToArray();

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
            HUDScreen.Instance.CompleteObjective(ObjectiveType.Engine);

            Boat.Engine.ChangeThrottle(input.Player.Move.ReadValue<Vector2>().x);

            if (!throttleAudioSource.isPlaying && allowThrottleSqueekPlayed)
            {
                if (Boat.Engine.Throttle > 0f)
                {
                    throttleAudioSource.volume = 0.1f;
                    throttleAudioSource.pitch = Random.Range(0.7f, 0.8f);
                    throttleAudioSource.Play();
                }
            }
        }

        else
        {
            throttleAudioSource.volume = 0;
            throttleAudioSource.Stop();

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
        if (_state == PlayerState.Throttle)
        {
            input.Player.Enable();
            TutorialScreen.Instance.ShowInputTooltip(TutorialType.Throttle);
        }

        else
        {
            input.Player.Disable();
            TutorialScreen.Instance.HideTutorial(TutorialType.Throttle);
        }
    }
}
