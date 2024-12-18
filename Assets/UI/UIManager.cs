using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public static event UnityAction<UIState> OnStateChanged;
    public static event UnityAction OnUIButtonClicked;
    public static void InvokeOnUIButtonClicked() => OnUIButtonClicked?.Invoke();
    public static event UnityAction OnUIButtonHovered;
    public static void InvokeOnUIButtonHovered() => OnUIButtonHovered?.Invoke();

    public UIState State { get; private set; } = UIState.TitleScreen;
    private readonly List<UIState> pauseScreens = new() { UIState.Pause, UIState.Options };
    private readonly List<UIState> slowmoScreens = new() { UIState.Formation, UIState.Fleet, UIState.PostCombat };
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
            DisableTab(screen.Root);
        }

        SetState(UIState.TitleScreen);
    }

    public static void DisableTab(VisualElement _target)
    {
        _target.tabIndex = -1;

        foreach (VisualElement child in _target.Children())
        {
            DisableTab(child);
        }
    }

    public void EnterFormationView()
    {
        SetState(UIState.Formation);
        FirstPersonController.Instance.SetState(PlayerState.Formation);
        CameraManager.Instance.SetState(CameraState.Formation);
    }

    public void ExitFormationView()
    {
        SetState(UIState.HUD);
        CommandScreen.Instance.ForceHide();
        FirstPersonController.Instance.SetState(PlayerState.FirstPerson);
        CameraManager.Instance.SetState(CameraState.Player);
    }

    public void ShowCommandView()
    {
        if (State != UIState.Formation)
        {
            CommandScreen.Instance.Show();
        }
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

        UnityEngine.Cursor.lockState = State != UIState.HUD ? CursorLockMode.None : CursorLockMode.Locked;
        UnityEngine.Cursor.visible = State != UIState.HUD;
    }

    private void Escape_performed(UnityEngine.InputSystem.InputAction.CallbackContext _obj)
    {
        switch (FirstPersonController.Instance.State)
        {
            case PlayerState.Cannon:
            case PlayerState.SteeringWheel:
            case PlayerState.Throttle:
            case PlayerState.Fleet:
                SetState(UIState.HUD);
                CameraManager.Instance.SetState(CameraState.Player);
                FirstPersonController.Instance.SetState(PlayerState.FirstPerson);
                return;
            case PlayerState.Formation:
                ExitFormationView();
                return;
        }

        switch (State)
        {
            case UIState.HUD when CommandScreen.Instance.State != CommandScreenState.Hidden:
                CommandScreen.Instance.ForceHide();
                return;

            case UIState.HUD:
                SetState(UIState.Pause);
                return;

            case UIState.Pause:
                SetState(UIState.HUD);
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
    Formation
}