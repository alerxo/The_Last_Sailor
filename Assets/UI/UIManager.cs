using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public static event UnityAction<UIState> OnStateChanged;

    public UIState State { get; private set; } = UIState.TitleScreen;
    private readonly List<UIState> pauseScreens = new() { UIState.Pause, UIState.Options };
    private readonly List<UIState> slowmoScreens = new() { UIState.Command, UIState.PostCombat };
    private UIState optionsReturnState;
    private bool isInTitleScreen = true;

    public static float UIScale = 1f;
    private const float UIScreenBaseWidth = 1920f;
    private const float UIScreenBaseHeight = 1080f;

    private InputSystem_Actions input;

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;

        UIScale = Mathf.Min(Screen.width / UIScreenBaseWidth, Screen.height / UIScreenBaseHeight);

        input = new();
        input.Player.Escape.performed += Escape_performed;
        input.Player.Enable();
    }

    private void OnDestroy()
    {
        input.Player.Disable();
    }

    private void Start()
    {
        foreach (UIScreen screen in FindObjectsByType<UIScreen>(FindObjectsSortMode.None))
        {
            screen.Generate();
        }

        SetState(UIState.TitleScreen);
    }

    public void SetStateOptions(UIState _returnState)
    {
        optionsReturnState = _returnState;
        SetState(UIState.Options);
    }

    public void ReturnFromOptions()
    {
        SetState(optionsReturnState);
    }

    public void SetState(UIState _state)
    {
        if (_state != UIState.TitleScreen && !pauseScreens.Contains(_state))
        {
            isInTitleScreen = false;
        }

        if (!isInTitleScreen && pauseScreens.Contains(_state)) Time.timeScale = 0;
        else if (!isInTitleScreen && slowmoScreens.Contains(_state)) Time.timeScale = 0.4f;
        else Time.timeScale = 1f;

        State = _state;
        OnStateChanged?.Invoke(State);

        Cursor.lockState = State != UIState.HUD ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = State != UIState.HUD;
    }

    private void Escape_performed(UnityEngine.InputSystem.InputAction.CallbackContext _obj)
    {
        switch (FirstPersonController.Instance.State)
        {
            case PlayerState.Cannon:
            case PlayerState.SteeringWheel:
            case PlayerState.Throttle:
            case PlayerState.Fleet:
            case PlayerState.Command:
                SetState(UIState.HUD);
                CameraManager.Instance.SetState(CameraState.Player);
                FirstPersonController.Instance.SetState(PlayerState.FirstPerson);
                return;
        }

        switch (State)
        {
            case UIState.HUD:
                SetState(UIState.Pause);
                return;

            case UIState.Pause:
                SetState(UIState.HUD);
                CameraManager.Instance.SetState(CameraState.Player);
                FirstPersonController.Instance.SetState(PlayerState.FirstPerson);
                return;
        }
    }
}

public enum UIState
{
    TitleScreen,
    HUD,
    Pause,
    Options,
    PostCombat,
    Fleet,
    Command
}
