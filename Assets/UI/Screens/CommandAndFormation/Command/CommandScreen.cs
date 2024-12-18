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
    private readonly Dictionary<Command, Button> commandButtons = new();
    private readonly Dictionary<Formation, Button> formationButtons = new();

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
        SetContent();
    }

    private void AdmiralController_OnCommandChanged(Command _command)
    {
        SetContent();
    }

    private void SetContent()
    {
        if (buttonContainer == null) return;

        foreach (Formation formation in formationButtons.Keys)
        {
            formationButtons[formation].SetEnabled(UIManager.Instance.State == UIState.Formation && PlayerBoatController.Instance.AdmiralController.Command == Command.Follow);
        }

        foreach (Command command in commandButtons.Keys)
        {
            if (command == PlayerBoatController.Instance.AdmiralController.Command)
            {
                SetBorderColor(commandButtons[command], GetMaterial(command).color);
                commandButtons[command].SetEnabled(false);
            }

            else
            {
                SetBorderColor(commandButtons[command], Color.black);
                commandButtons[command].SetEnabled(true);
            }
        }
    }

    private Material GetMaterial(Command _command)
    {
        switch (_command)
        {
            case Command.Follow:
                return formationMaterial;

            case Command.Wait:
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
        SetMargin(buttonContainer, 0, 50, 50, 0);
        buttonContainer.pickingMode = PickingMode.Ignore;
        container.Add(buttonContainer);

        VisualElement followContainer = new();
        followContainer.AddToClassList("command-follow-container");
        SetMargin(followContainer, 0, 30, 0, 0);
        buttonContainer.Add(followContainer);

        CreateFormationButtonContainer(followContainer);

        CreateButton(followContainer, "1", $"{Command.Follow}", "Ships in fleet will follow the\nplayer in given formation", Command.Follow);
        CreateButton(buttonContainer, "2", $"{Command.Wait}", "Ships in fleet will wait at\ncurrent position in given formation ", Command.Wait);
        CreateButton(buttonContainer, "3", $"{Command.Charge}", "Ships in fleet will charge the\nclosest enemy", Command.Charge);

        AdmiralController_OnCommandChanged(PlayerBoatController.Instance.AdmiralController.Command);

        HiddenState();
    }

    private void CreateButton(VisualElement _parent, string _input, string _name, string _description, Command _command)
    {
        Button button = new(() => admiralController.SetCommandForSubordinates(_command));
        button.AddToClassList("main-button");
        button.AddToClassList("command-button");
        button.pickingMode = PickingMode.Position;
        SetWidth(button, 300);
        SetMargin(button, 0, _command == Command.Follow ? 0 : 30, 0, 0);
        SetBorderWidthRadius(button, 5, 10);
        _parent.Add(button);

        VisualElement headerContainer = new();
        headerContainer.AddToClassList("command-button-header-container");
        button.Add(headerContainer);

        Label inputLabel = new(_input);
        inputLabel.AddToClassList("command-button-input");
        SetMargin(inputLabel, 0, 0, 0, 4);
        SetPadding(inputLabel, 0, 0, 15, 15);
        SetBorderWidthRadius(inputLabel, 4, 7);
        SetFontSize(inputLabel, 30);
        headerContainer.Add(inputLabel);

        Label header = new(_name);
        header.AddToClassList("command-button-text");
        SetFontSize(header, 26);
        headerContainer.Add(header);

        Label description = new(_description);
        description.AddToClassList("command-button-text");
        SetFontSize(description, 19);
        button.Add(description);

        commandButtons[_command] = button;
    }

    private void CreateFormationButtonContainer(VisualElement _parent)
    {
        VisualElement container = new();
        container.AddToClassList("command-formation-container");
        SetMargin(container, 0, 0, 25, 0);
        _parent.Add(container);

        CreateFormationButton(container, "-", Formation.Line);
        CreateFormationButton(container, ">", Formation.Spearhead);
        CreateFormationButton(container, "O", Formation.Ring);
    }

    private void CreateFormationButton(VisualElement _parent, string _text, Formation _formation)
    {
        Button button = new(() => PlayerBoatController.Instance.AdmiralController.SetDefaultFormation(_formation));
        button.AddToClassList("main-button");
        button.AddToClassList("command-formation-button");
        SetSize(button, 32, 32);
        SetBorderWidthRadius(button, 3, 7);
        SetFontSize(button, 30);
        button.pickingMode = PickingMode.Position;
        button.text = _text;
        button.SetEnabled(UIManager.Instance.State == UIState.Formation && PlayerBoatController.Instance.AdmiralController.Command == Command.Follow);
        _parent.Add(button);

        formationButtons[_formation] = button;
    }
}

public enum CommandScreenState
{
    Visible,
    Fading,
    Hidden
}