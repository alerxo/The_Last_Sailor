using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public static event UnityAction<UIState> OnStateChanged;

    public UIState State { get; private set; } = UIState.MainMenu;

    public static float UIScale = 1f;
    private const float UIScreenBaseWidth = 1920f;
    private const float UIScreenBaseHeight = 1080f;

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;

        UIScale = Mathf.Min(Screen.width / UIScreenBaseWidth, Screen.height / UIScreenBaseHeight);
    }

    private void Start()
    {
        SetState(UIState.MainMenu);
    }

    public void SetState(UIState _state)
    {
        State = _state;
        OnStateChanged?.Invoke(State);
    }
}

public enum UIState
{
    MainMenu,
    Game
}
