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

    private readonly List<UIState> hiddenCursorStates = new() { UIState.None, UIState.HUD };

    private UIState State = UIState.TitleScreen;
    private readonly List<UIState> pauseScreens = new() { UIState.Pause, UIState.Options, UIState.Controls };
    private UIState optionsReturnState, controlsReturnState;
    private bool isInTitleScreen = true;

    public static float UIScale = 1f;
    private const float UIScreenBaseWidth = 1920f;
    private const float UIScreenBaseHeight = 1080f;

    private InputSystem_Actions input;

    private UIState toggleUIState = UIState.None;

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;

        UIScale = Mathf.Min(Screen.width / UIScreenBaseWidth, Screen.height / UIScreenBaseHeight);

        input = new();
        input.Player.Escape.performed += Escape_performed;
        input.Player.Tab.performed += Tab_performed;
#if UNITY_EDITOR
        input.Player.ToggleUI.performed += ToggleUI_performed;
#endif
        input.Player.Enable();
    }

    private void OnDestroy()
    {
        input.Player.Escape.performed -= Escape_performed;
        input.Player.Tab.performed -= Tab_performed;
#if UNITY_EDITOR
        input.Player.ToggleUI.performed -= ToggleUI_performed;
#endif
        input.Player.Disable();
    }

    private void Start()
    {
        foreach (UIScreen screen in FindObjectsByType<UIScreen>(FindObjectsSortMode.None))
        {
            screen.Generate();
            DisableTab(screen.Root);
        }

        SetState(UIState.TitleScreen, true);
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
        HUDScreen.Instance.ForceHideCommand();
        FirstPersonController.Instance.SetState(PlayerState.FirstPerson);
        CameraManager.Instance.SetState(CameraState.Player);
    }

    public void ShowCommandView()
    {
        if (GetState() != UIState.Formation)
        {
            HUDScreen.Instance.ShowCommand();
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

    public void SetStateControls(UIState _returnState)
    {
        controlsReturnState = _returnState;
        SetState(UIState.Controls);
    }

    public void ReturnFromControls()
    {
        SetState(controlsReturnState);
    }

#if UNITY_EDITOR

    private void ToggleUI_performed(UnityEngine.InputSystem.InputAction.CallbackContext _obj)
    {
        UIState current = State;
        State = toggleUIState;
        toggleUIState = current;

        Time.timeScale = (!isInTitleScreen && pauseScreens.Contains(GetState())) ? 0 : 1;
        UnityEngine.Cursor.lockState = !hiddenCursorStates.Contains(GetState()) ? CursorLockMode.None : CursorLockMode.Locked;
        UnityEngine.Cursor.visible = !hiddenCursorStates.Contains(GetState());

        OnStateChanged?.Invoke(GetState());
    }

#endif

    public void SetState(UIState _state, bool _isForceMode = false)
    {
        if (State == UIState.None)
        {
            UnityEngine.Cursor.lockState = !hiddenCursorStates.Contains(_state) ? CursorLockMode.None : CursorLockMode.Locked;
            UnityEngine.Cursor.visible = !hiddenCursorStates.Contains(_state);
            Time.timeScale = (!isInTitleScreen && pauseScreens.Contains(_state)) ? 0 : 1;

            toggleUIState = _state;
            OnStateChanged?.Invoke(_state);

            return;
        }

        if (_state != UIState.TitleScreen && _state != UIState.Credits && !pauseScreens.Contains(_state))
        {
            isInTitleScreen = false;
        }

        UnityEngine.Cursor.lockState = !hiddenCursorStates.Contains(_state) ? CursorLockMode.None : CursorLockMode.Locked;
        UnityEngine.Cursor.visible = !hiddenCursorStates.Contains(_state);
        Time.timeScale = (!isInTitleScreen && pauseScreens.Contains(_state)) ? 0 : 1;

        State = _state;
        OnStateChanged?.Invoke(_state);
    }

    public UIState GetState(bool _isForceMode = false)
    {
        return State == UIState.None && !_isForceMode ? toggleUIState : State;
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

        switch (GetState())
        {
            case UIState.HUD when HUDScreen.Instance.CommandState != CommandObjectiveState.Hidden:
                HUDScreen.Instance.ForceHideCommand();
                return;

            case UIState.Options:
                ReturnFromOptions();
                return;

            case UIState.Controls:
                ReturnFromControls();
                return;

            case UIState.HUD:
                SetState(UIState.Pause);
                return;

            case UIState.Pause:
                SetState(UIState.HUD);
                return;
        }
    }

    private void Tab_performed(UnityEngine.InputSystem.InputAction.CallbackContext _obj)
    {
        HUDScreen.Instance.ShowCommand();
        HUDScreen.Instance.ShowObjective();
    }
}

public enum UIState
{
    None,
    TitleScreen,
    HUD,
    Pause,
    Options,
    Controls,
    PostCombat,
    Fleet,
    Formation,
    Credits
}