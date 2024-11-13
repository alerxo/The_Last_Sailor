using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }
    public static event UnityAction<CameraState> OnStateChanged;
    public CameraState State { get; private set; }

    [SerializeField] private CinemachineCamera mainMenuCamera, playerCamera, steeringWheelCamera, interactionCamera;
    [SerializeField] private CinemachineBasicMultiChannelPerlin[] cinemachineBasicMultiChannelPerlins;
    [SerializeField] private CinemachineInputAxisController[] cinemachineInputAxisControllers;

    private Transform interactionTarget;

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;

        UIManager.OnStateChanged += UIManager_OnStateChanged;
    }

    private void OnDestroy()
    {
        UIManager.OnStateChanged -= UIManager_OnStateChanged;
    }

    private void Start()
    {
        SetState(CameraState.MainMenu);
    }

    private void Update()
    {
        if (interactionTarget != null)
        {
            SetInteractionCameraPosition();
        }
    }

    public void SetInteractionCamera(Transform _target)
    {
        interactionCamera.Target.TrackingTarget = _target;
        interactionTarget = _target;
        SetInteractionCameraPosition();
        SetState(CameraState.Interaction);
    }

    public void SetInteractionCameraPosition()
    {
        interactionCamera.ForceCameraPosition(interactionTarget.position, interactionTarget.rotation);
    }

    public void SetState(CameraState _state)
    {
        State = _state;

        switch (State)
        {
            case CameraState.MainMenu:
                interactionTarget = null;
                break;

            case CameraState.Player:
                playerCamera.ForceCameraPosition(playerCamera.transform.position, playerCamera.transform.rotation);
                interactionTarget = null;
                break;

            case CameraState.SteeringWheel:
                steeringWheelCamera.ForceCameraPosition(steeringWheelCamera.transform.position, steeringWheelCamera.transform.rotation);
                interactionTarget = null;
                break;
        }

        mainMenuCamera.enabled = State == CameraState.MainMenu;
        playerCamera.enabled = State == CameraState.Player;
        steeringWheelCamera.enabled = State == CameraState.SteeringWheel;
        interactionCamera.enabled = State == CameraState.Interaction;

        OnStateChanged?.Invoke(State);
    }

    public void ShakeCamera(float _intensity, float _time)
    {
        StopCoroutine(CameraShake(_intensity, _time));
        StartCoroutine(CameraShake(_intensity, _time));
    }

    private IEnumerator CameraShake(float _intensity, float _time)
    {
        foreach (CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin in cinemachineBasicMultiChannelPerlins)
        {
            cinemachineBasicMultiChannelPerlin.AmplitudeGain = _intensity;
            cinemachineBasicMultiChannelPerlin.FrequencyGain = _intensity;
        }

        yield return new WaitForSeconds(_time);

        foreach (CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin in cinemachineBasicMultiChannelPerlins)
        {
            cinemachineBasicMultiChannelPerlin.AmplitudeGain = 0;
            cinemachineBasicMultiChannelPerlin.FrequencyGain = 0;
        }
    }

    private void UIManager_OnStateChanged(UIState _state)
    {
        switch (_state)
        {
            case UIState.TitleScreen:
            case UIState.Pause:

                foreach (CinemachineInputAxisController cinemachineInputAxisController in cinemachineInputAxisControllers)
                {
                    cinemachineInputAxisController.enabled = false;
                }

                break;

            case UIState.HUD:

                foreach (CinemachineInputAxisController cinemachineInputAxisController in cinemachineInputAxisControllers)
                {
                    cinemachineInputAxisController.enabled = true;
                }

                break;
        }
    }
}

public enum CameraState
{
    MainMenu,
    Player,
    SteeringWheel,
    Interaction
}
