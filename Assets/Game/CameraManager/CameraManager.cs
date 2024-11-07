using System.Collections;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }
    public static event UnityAction<CameraState> OnStateChanged;
    public CameraState State { get; private set; }

    [SerializeField] private CinemachineCamera mainMenuCamera, playerCamera, boatCamera, cannonCamera;
    [SerializeField] private CinemachineBasicMultiChannelPerlin[] cinemachineBasicMultiChannelPerlins;

    private InputSystem_Actions input;

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;

        input = new();
        input.Player.Escape.performed += Escape_performed;
    }

    private void OnDestroy()
    {
        input.Player.Escape.performed -= Escape_performed;
    }

    private void Start()
    {
        SetState(CameraState.MainMenu);
    }

    public void SetCannonCamera(Transform _target)
    {
        cannonCamera.Target.TrackingTarget = _target;
        SetCannonCameraPosition(_target);
        SetState(CameraState.Cannon);
    }

    public void SetCannonCameraPosition(Transform _target)
    {
        cannonCamera.ForceCameraPosition(_target.position, _target.rotation);
    }

    public void SetState(CameraState _state)
    {
        State = _state;

        switch (State)
        {
            case CameraState.MainMenu:
                input.Player.Disable();
                break;

            case CameraState.Player:
                playerCamera.ForceCameraPosition(playerCamera.transform.position, playerCamera.transform.rotation);
                input.Player.Disable();
                break;

            case CameraState.Boat:
                boatCamera.ForceCameraPosition(boatCamera.transform.position, boatCamera.transform.rotation);
                input.Player.Enable();
                break;

            case CameraState.Cannon:
                input.Player.Enable();
                break;
        }

        mainMenuCamera.enabled = State == CameraState.MainMenu;
        playerCamera.enabled = State == CameraState.Player;
        boatCamera.enabled = State == CameraState.Boat;
        cannonCamera.enabled = State == CameraState.Cannon;

        Cursor.lockState = State == CameraState.MainMenu ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = State == CameraState.MainMenu;

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

    private void Escape_performed(UnityEngine.InputSystem.InputAction.CallbackContext _obj)
    {
        switch (State)
        {
            case CameraState.Boat:
            case CameraState.Cannon:
                SetState(CameraState.Player);
                break;
        }
    }
}

public enum CameraState
{
    MainMenu,
    Player,
    Boat,
    Cannon
}
