using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

public class CommandScreen : UIScreen
{
    public static CommandScreen Instance { get; private set; }

    protected override List<UIState> ActiveStates => new() { UIState.Formation, UIState.HUD };

    public CommandScreenState State { get; private set; } = CommandScreenState.Hidden;
    private float stateTimer = 0;
    private const float TIME_SHOWING = 2f;
    private const float TIME_FADING = 1f;

    private PlayerAdmiralController admiralController;
    [SerializeField] private Material defaultMaterial, formationMaterial, holdMaterial, chargeMaterial;

    private VisualElement buttonContainer;
    private Button changeViewButton;
    private readonly Dictionary<Command, Button> commandButtons = new();

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;

        UIManager.OnStateChanged += UIManager_OnStateChanged;
    }

    private void Start()
    {
        admiralController = PlayerBoatController.Instance.AdmiralController;
        admiralController.OnCommandChanged += AdmiralController_OnCommandChanged;
    }

    private void OnDestroy()
    {
        UIManager.OnStateChanged -= UIManager_OnStateChanged;
    }

    private void Update()
    {
        if (UIManager.Instance.State == UIState.Formation)
        {
            Show();
        }

        switch (State)
        {
            case CommandScreenState.Visible:
                ShowingState();
                break;

            case CommandScreenState.Fading:
                FadingState();
                break;

            case CommandScreenState.Hidden:
                HiddenState();
                break;
        }
    }

    public void Show()
    {
        stateTimer = TIME_SHOWING;
        State = CommandScreenState.Visible;
    }

    private void ShowingState()
    {
        if ((stateTimer -= Time.deltaTime) <= 0)
        {
            stateTimer = TIME_FADING;
            State = CommandScreenState.Fading;
        }

        else
        {
            buttonContainer.style.opacity = 1;
        }
    }

    private void FadingState()
    {
        if ((stateTimer -= Time.deltaTime) <= 0)
        {
            State = CommandScreenState.Hidden;
        }

        else
        {
            buttonContainer.style.opacity = stateTimer / TIME_FADING;
        }
    }

    private void HiddenState()
    {
        buttonContainer.style.opacity = 0;
    }

    public void ForceHide()
    {
        State = CommandScreenState.Hidden;
    }

    private void UIManager_OnStateChanged(UIState _state)
    {
        changeViewButton.text = GetChangeViewText();
    }

    private void AdmiralController_OnCommandChanged(Command _command)
    {
        foreach (Command command in commandButtons.Keys)
        {
            if (command == _command)
            {
                SetBorder(commandButtons[command], GetMaterial(command).color);
                commandButtons[command].SetEnabled(false);
            }

            else
            {
                SetBorder(commandButtons[command], defaultMaterial.color);
                commandButtons[command].SetEnabled(true);
            }
        }
    }

    private Material GetMaterial(Command _command)
    {
        switch (_command)
        {
            case Command.Formation:
                return formationMaterial;

            case Command.Hold:
                return holdMaterial;

            case Command.Charge:
                return chargeMaterial;

            default:
                Debug.LogError("Defaulted");
                return null;
        }
    }

    public override void Generate()
    {
        VisualElement container = new();
        container.AddToClassList("command-container");
        container.pickingMode = PickingMode.Ignore;
        Root.Add(container);

        buttonContainer = new();
        buttonContainer.AddToClassList("command-button-container");
        buttonContainer.style.opacity = 0;
        buttonContainer.pickingMode = PickingMode.Ignore;
        container.Add(buttonContainer);

        CreateTopButtons(buttonContainer);
        CreateChangeViewButton(buttonContainer);
    }

    private void CreateTopButtons(VisualElement _parent)
    {
        VisualElement container = new();
        container.AddToClassList("command-top-button-container");
        _parent.Add(container);

        CreateTopButton(container, $"1: {Command.Formation}", Command.Formation);
        CreateTopButton(container, $"2: {Command.Hold}", Command.Hold);
        CreateTopButton(container, $"3: {Command.Charge}", Command.Charge);

        AdmiralController_OnCommandChanged(PlayerBoatController.Instance.AdmiralController.Command);
    }

    private void CreateTopButton(VisualElement _parent, string _name, Command _command)
    {
        Button button = new(() => admiralController.SetCommandForSubordinates(_command));
        button.AddToClassList("main-button");
        button.AddToClassList("command-top-button");
        button.pickingMode = PickingMode.Position;
        SetFontSize(button, 26);
        button.text = _name;
        _parent.Add(button);

        commandButtons[_command] = button;
    }

    private void CreateChangeViewButton(VisualElement _parent)
    {
        changeViewButton = new(OnChangeView);
        changeViewButton.AddToClassList("command-change-view-button");
        changeViewButton.pickingMode = PickingMode.Position;
        SetFontSize(changeViewButton, 30);
        changeViewButton.text = GetChangeViewText();
        _parent.Add(changeViewButton);
    }

    private void OnChangeView()
    {
        if (UIManager.Instance.State == UIState.Formation) UIManager.Instance.ExitFormationView();
        else UIManager.Instance.EnterFormationView();
    }

    private string GetChangeViewText() => $"4: {(UIManager.Instance.State == UIState.Formation ? "Exit" : "Enter")} Formation View";
}

public enum CommandScreenState
{
    Visible,
    Fading,
    Hidden
}