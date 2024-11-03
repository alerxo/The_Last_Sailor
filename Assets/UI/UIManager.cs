using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public static event UnityAction<UIState> OnStateChanged;

    public UIState State { get; private set; } = UIState.MainMenu;

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;
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
