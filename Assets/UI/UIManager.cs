using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public static event UnityAction<UIState> OnStateChanged;

    public UIState State { get; private set; } = UIState.TitleScreen;

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

    public void SetState(UIState _state)
    {
        State = _state;
        OnStateChanged?.Invoke(State);

        Cursor.lockState = State != UIState.HUD ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = State != UIState.HUD;
    }

    private void Escape_performed(UnityEngine.InputSystem.InputAction.CallbackContext _obj)
    {
        switch (State)
        {
            case UIState.HUD:
                SetState(UIState.Pause);
                break;

            case UIState.Pause:
                SetState(UIState.HUD);
                break;
        }
    }
}

public enum UIState
{
    TitleScreen,
    HUD,
    Pause,
    PostCombat
}
