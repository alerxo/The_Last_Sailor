using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }
    public static event UnityAction<CameraState> OnStateChanged;
    public static CameraState CameraState { get; private set; }

    public CameraState State { get; private set; }

    [SerializeField] private CinemachineCamera mainMenuCamera, playerCamera, boatCamera;
    [SerializeField] private CinemachineBasicMultiChannelPerlin[] cinemachineBasicMultiChannelPerlins;

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;
    }

    private void Start()
    {
        SetState(CameraState.MainMenu);
    }

    public void SetState(CameraState _state)
    {
        State = _state;

        switch (State)
        {
            case CameraState.Player:
                playerCamera.ForceCameraPosition(playerCamera.transform.position, playerCamera.transform.rotation);
                break;

            case CameraState.Boat:
                boatCamera.ForceCameraPosition(boatCamera.transform.position, boatCamera.transform.rotation);
                break;
        }

        mainMenuCamera.enabled = State == CameraState.MainMenu;
        playerCamera.enabled = State == CameraState.Player;
        boatCamera.enabled = State == CameraState.Boat;

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
}

public enum CameraState
{
    MainMenu,
    Player,
    Boat
}
