using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Assertions;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    public CameraState State { get; private set; }

    [SerializeField] private CinemachineCamera mainMenuCamera, playerCamera;
    [SerializeField] private CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin;

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
        }

        mainMenuCamera.enabled = State == CameraState.MainMenu;
        playerCamera.enabled = State == CameraState.Player;
        Cursor.lockState = State == CameraState.Player ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = State != CameraState.Player;
    }

    public void ShakeCamera(float _intensity, float _time)
    {
        StopCoroutine(CameraShake(_intensity, _time));
        StartCoroutine(CameraShake(_intensity, _time));
    }

    private IEnumerator CameraShake(float _intensity, float _time)
    {
        cinemachineBasicMultiChannelPerlin.AmplitudeGain = _intensity;
        cinemachineBasicMultiChannelPerlin.FrequencyGain = _intensity;

        yield return new WaitForSeconds(_time);

        cinemachineBasicMultiChannelPerlin.AmplitudeGain = 0;
        cinemachineBasicMultiChannelPerlin.FrequencyGain = 0;
    }
}

public enum CameraState
{
    MainMenu,
    Player
}
