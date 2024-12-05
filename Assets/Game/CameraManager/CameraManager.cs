using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }
    public static event UnityAction<CameraState> OnStateChanged;
    public CameraState State { get; private set; }

    private InputSystem_Actions input;

    private const float MAX_COMMAND_ZOOM = 150;
    private const float COMMAND_SROLL_SPEED = 350;
    private const float MAX_COMMAND_MOVEMENT = 100;
    private const float COMMAND_MOVEMENT_SPEED = 150;
    private Vector3 commandCameraMovement;

    private CinemachineCamera mainMenuCamera;
    public CinemachineCamera PlayerCamera { get; private set; }
    private CinemachineCamera steeringWheelCamera;
    private CinemachineCamera interactionCamera;
    private CinemachineCamera[] fleetCameras;
    private CinemachineCamera[] formationCameras;

    private CinemachineBasicMultiChannelPerlin[] cinemachineBasicMultiChannelPerlins;
    private CinemachineInputAxisController[] cinemachineInputAxisControllers;

    private FirstPersonController player;

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;

        input = new();
        input.Player.Enable();

        AssignCameras();

        cinemachineBasicMultiChannelPerlins = FindObjectsByType<CinemachineBasicMultiChannelPerlin>(FindObjectsSortMode.None);
        cinemachineInputAxisControllers = FindObjectsByType<CinemachineInputAxisController>(FindObjectsSortMode.None);

        UIManager.OnStateChanged += UIManager_OnStateChanged;
    }

    private void AssignCameras()
    {
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

        GameObject[] formation = GameObject.FindGameObjectsWithTag("FormationCamera");
        formationCameras = new CinemachineCamera[formation.Length];

        for (int i = 0; i < formation.Length; i++)
        {
            formationCameras[i] = formation[i].GetComponent<CinemachineCamera>();
        }
    }

    private void Start()
    {
        SetState(CameraState.MainMenu);
        player = FirstPersonController.Instance;
    }

    private void OnDestroy()
    {
        input.Player.Disable();

        UIManager.OnStateChanged -= UIManager_OnStateChanged;
    }

    private void Update()
    {
        switch (State)
        {
            case CameraState.Formation:
                GetFormationCameraMovement();
                SetFormationCameraPosition();
                break;
        }
    }

    public void SetInteractionCamera(Transform _target, IInteractable interactable)
    {
        interactionCamera.Target.TrackingTarget = _target;
        SetState(CameraState.Interaction);

        Vector3 position = interactable.Position;
        position.y = player.transform.position.y;
        player.Rigidbody.Move(position, player.transform.rotation);
        PlayerCamera.ForceCameraPosition(PlayerCamera.transform.position, _target.transform.rotation);
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

    public void FocusFormationCamera(Vector3 _position)
    {
        Vector3 position = PlayerBoatController.Instance.transform.InverseTransformVector(_position - PlayerBoatController.Instance.transform.position);
        position.y = commandCameraMovement.y;
        commandCameraMovement = position;
        GetNextFormationCamera();
    }

    public void GetFormationCameraMovement()
    {
        Vector2 inputVector = input.Player.Move.ReadValue<Vector2>();

        float movementSpeed = COMMAND_MOVEMENT_SPEED * Mathf.Lerp(1, 2, commandCameraMovement.y / MAX_COMMAND_ZOOM);
        Vector3 movement = new(inputVector.x * movementSpeed, input.Player.CameraZoom.ReadValue<float>() * COMMAND_SROLL_SPEED, inputVector.y * movementSpeed);
        movement *= Time.deltaTime;
        movement += commandCameraMovement;

        commandCameraMovement.x = Mathf.Clamp(movement.x, -MAX_COMMAND_MOVEMENT, MAX_COMMAND_MOVEMENT);
        commandCameraMovement.y = Mathf.Clamp(movement.y, 0, MAX_COMMAND_ZOOM);
        commandCameraMovement.z = Mathf.Clamp(movement.z, -MAX_COMMAND_MOVEMENT, MAX_COMMAND_MOVEMENT);
    }

    public void SetFormationCameraPosition()
    {
        Vector3 playerXZPosition = PlayerBoatController.Instance.transform.position;
        playerXZPosition.y = 0;
        Vector3 position = playerXZPosition + PlayerBoatController.Instance.transform.TransformVector(commandCameraMovement);
        Vector3 rotation = new(
            GetCurrentCommandCamera().Target.TrackingTarget.transform.rotation.eulerAngles.x,
            PlayerBoatController.Instance.transform.rotation.eulerAngles.y,
            GetCurrentCommandCamera().Target.TrackingTarget.transform.rotation.eulerAngles.z);

        GetCurrentCommandCamera().Target.TrackingTarget.SetPositionAndRotation(position, Quaternion.Euler(rotation));
    }

    public void ResetFormationCameras()
    {
        commandCameraMovement = Vector3.zero;

        Vector3 playerXZPosition = PlayerBoatController.Instance.transform.position;
        playerXZPosition.y = 0;
        Vector3 position = playerXZPosition + PlayerBoatController.Instance.transform.TransformVector(commandCameraMovement);
        Vector3 rotation = new(
            GetCurrentCommandCamera().Target.TrackingTarget.transform.rotation.eulerAngles.x,
            PlayerBoatController.Instance.transform.rotation.eulerAngles.y,
            GetCurrentCommandCamera().Target.TrackingTarget.transform.rotation.eulerAngles.z);

        foreach (CinemachineCamera cinemachine in formationCameras)
        {
            cinemachine.Target.TrackingTarget.SetPositionAndRotation(position, Quaternion.Euler(rotation));
        }
    }

    public CinemachineCamera GetCurrentCommandCamera()
    {
        return formationCameras[0].enabled ? formationCameras[0] : formationCameras[1];
    }

    public void GetNextFormationCamera()
    {
        if (formationCameras[0].enabled)
        {
            formationCameras[0].enabled = false;
            formationCameras[1].enabled = true;
        }

        else
        {
            formationCameras[1].enabled = false;
            formationCameras[0].enabled = true;
        }
    }

    public void SetState(CameraState _state)
    {
        switch (_state)
        {
            case CameraState.Player when State != CameraState.MainMenu:
                PlayerCamera.ForceCameraPosition(PlayerCamera.transform.position, PlayerCamera.transform.rotation);
                break;

            case CameraState.SteeringWheel:
                steeringWheelCamera.ForceCameraPosition(steeringWheelCamera.transform.position, steeringWheelCamera.transform.rotation);
                break;
        }

        if (_state != CameraState.Fleet)
        {
            foreach (CinemachineCamera camera in fleetCameras)
            {
                camera.enabled = false;
            }
        }

        if (_state != CameraState.Formation)
        {
            foreach (CinemachineCamera camera in formationCameras)
            {
                camera.enabled = false;
            }
        }

        else
        {
            ResetFormationCameras();
            GetNextFormationCamera();
        }

        mainMenuCamera.enabled = _state == CameraState.MainMenu;
        PlayerCamera.enabled = _state == CameraState.Player;
        steeringWheelCamera.enabled = _state == CameraState.SteeringWheel;
        interactionCamera.enabled = _state == CameraState.Interaction;

        State = _state;

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
    Formation
}
