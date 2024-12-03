using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.Rendering;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }
    public static event UnityAction<CameraState> OnStateChanged;
    public CameraState State { get; private set; }

    private CinemachineCamera mainMenuCamera;
    public CinemachineCamera PlayerCamera { get; private set; }
    private CinemachineCamera steeringWheelCamera;
    private CinemachineCamera interactionCamera;
    private CinemachineCamera[] fleetCameras;
    private CinemachineCamera commandCamera;

    private CinemachineBasicMultiChannelPerlin[] cinemachineBasicMultiChannelPerlins;
    private CinemachineInputAxisController[] cinemachineInputAxisControllers;

    private Transform interactionTarget;
    private FirstPersonController player;

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;

        mainMenuCamera = GameObject.FindWithTag("MainMenuCamera").GetComponent<CinemachineCamera>();

        PlayerCamera = GameObject.FindWithTag("PlayerCamera").GetComponent<CinemachineCamera>();
        PlayerCamera.Target.TrackingTarget = GameObject.FindWithTag("PlayerCameraTarget").transform;

        steeringWheelCamera = GameObject.FindWithTag("SteeringWheelCamera").GetComponent<CinemachineCamera>();
        steeringWheelCamera.Target.TrackingTarget = GameObject.FindWithTag("SteeringWheelCameraTarget").transform;

        interactionCamera = GameObject.FindWithTag("InteractionCamera").GetComponent<CinemachineCamera>();

        GameObject[] fleet = GameObject.FindGameObjectsWithTag("FleetCamera");
        fleetCameras = new CinemachineCamera[fleet.Length];

        for (int i = 0; i < fleet.Length; i++)
        {
            fleetCameras[i] = fleet[i].GetComponent<CinemachineCamera>();
        }

        commandCamera = GameObject.FindWithTag("CommandCamera").GetComponent<CinemachineCamera>();

        cinemachineBasicMultiChannelPerlins = FindObjectsByType<CinemachineBasicMultiChannelPerlin>(FindObjectsSortMode.None);
        cinemachineInputAxisControllers = FindObjectsByType<CinemachineInputAxisController>(FindObjectsSortMode.None);

        UIManager.OnStateChanged += UIManager_OnStateChanged;
    }

    private void Start()
    {
        SetState(CameraState.MainMenu);
        player = FirstPersonController.Instance;
    }

    private void OnDestroy()
    {
        UIManager.OnStateChanged -= UIManager_OnStateChanged;
    }

    private void Update()
    {
        switch (State)
        {
            case CameraState.Interaction when interactionTarget != null:
                SetInteractionCameraPosition();
                break;

            case CameraState.Command:
                SetCommandCameraPosition();
                break;
        }
    }

    public void SetInteractionCamera(Transform _target, IInteractable interactable)
    {
        interactionCamera.Target.TrackingTarget = _target;
        interactionTarget = _target;
        SetInteractionCameraPosition();
        SetState(CameraState.Interaction);

        Vector3 position = interactable.Position;
        position.y = player.transform.position.y;
        player.Rigidbody.Move(position, player.transform.rotation);
        PlayerCamera.ForceCameraPosition(PlayerCamera.transform.position, interactionTarget.transform.rotation);
    }

    public void SetInteractionCameraPosition()
    {
        interactionCamera.ForceCameraPosition(interactionTarget.position, interactionTarget.rotation);
    }

    public void SetFleetCamera(Transform _target)
    {
        GetNextFleetCamera().Target.TrackingTarget = _target;
        SetState(CameraState.Fleet);
    }

    public CinemachineCamera GetNextFleetCamera()
    {
        if (fleetCameras[0].enabled)
        {
            fleetCameras[0].enabled = false;
            fleetCameras[1].enabled = true;

            return fleetCameras[1];
        }

        fleetCameras[1].enabled = false;
        fleetCameras[0].enabled = true;

        return fleetCameras[0];
    }

    public void SetCommandCameraPosition()
    {
        commandCamera.Target.TrackingTarget.position = PlayerBoatController.Instance.transform.position;
    }

    public void SetState(CameraState _state)
    {
        State = _state;

        if (State != CameraState.Interaction && interactionTarget != null)
        {
            PlayerCamera.ForceCameraPosition(PlayerCamera.transform.position, interactionTarget.transform.rotation);
            interactionTarget = null;
        }

        switch (State)
        {
            case CameraState.Player:
                PlayerCamera.ForceCameraPosition(PlayerCamera.transform.position, PlayerCamera.transform.rotation);
                break;

            case CameraState.SteeringWheel:
                steeringWheelCamera.ForceCameraPosition(steeringWheelCamera.transform.position, steeringWheelCamera.transform.rotation);
                break;
        }

        mainMenuCamera.enabled = State == CameraState.MainMenu;
        PlayerCamera.enabled = State == CameraState.Player;
        steeringWheelCamera.enabled = State == CameraState.SteeringWheel;
        interactionCamera.enabled = State == CameraState.Interaction;
        commandCamera.enabled = State == CameraState.Command;

        if (State != CameraState.Fleet)
        {
            foreach (CinemachineCamera camera in fleetCameras)
            {
                camera.enabled = false;
            }
        }

        OnStateChanged?.Invoke(State);
    }

    public void ShakeCamera(float _amplitude, float _frequency, float _time, float _windUp)
    {
        StopAllCoroutines();
        StartCoroutine(CameraShake(_amplitude, _frequency, _time, _windUp));
    }

    private IEnumerator CameraShake(float _amplitude, float _frequency, float _time, float windUp)
    {
        float duration = 0;

        while ((duration += Time.deltaTime) < windUp)
        {
            foreach (CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin in cinemachineBasicMultiChannelPerlins)
            {
                cinemachineBasicMultiChannelPerlin.AmplitudeGain = Mathf.Lerp(0, _amplitude, duration / windUp);
                cinemachineBasicMultiChannelPerlin.FrequencyGain = Mathf.Lerp(0, _frequency, duration / windUp);
            }
        }

        foreach (CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin in cinemachineBasicMultiChannelPerlins)
        {
            cinemachineBasicMultiChannelPerlin.AmplitudeGain = _amplitude;
            cinemachineBasicMultiChannelPerlin.FrequencyGain = _frequency;
        }

        yield return new WaitForSeconds(_time);

        duration = 0;

        while ((duration += Time.deltaTime) < windUp)
        {
            foreach (CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin in cinemachineBasicMultiChannelPerlins)
            {
                cinemachineBasicMultiChannelPerlin.AmplitudeGain = Mathf.Lerp(_amplitude, 0, duration / windUp);
                cinemachineBasicMultiChannelPerlin.FrequencyGain = Mathf.Lerp(_frequency, 0, duration / windUp);
            }
        }

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
    Interaction,
    Fleet,
    Command
}
