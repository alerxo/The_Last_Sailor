using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

public class TutorialScreen : UIScreen
{
    public static TutorialScreen Instance { get; private set; }

    protected override List<UIState> ActiveStates => new() { UIState.HUD, UIState.Fleet, UIState.Formation };

    private readonly List<TutorialType> ignore = new();
    private readonly List<TutorialType> current = new();

    private VisualElement inputContainer;
    private VisualElement menuContainer;

    private InputSystem_Actions input;

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;

        input = new();
        input.Player.HideTooltip.performed += HideTooltip_performed;
        input.Player.Enable();

        UIManager.OnStateChanged += UIManager_OnStateChanged;
    }

    private void OnDestroy()
    {
        input.Player.HideTooltip.performed -= HideTooltip_performed;
        input.Player.Disable();

        UIManager.OnStateChanged -= UIManager_OnStateChanged;
    }

    private void UIManager_OnStateChanged(UIState _state)
    {
        switch (_state)
        {
            case UIState.Formation:
                ShowInputTooltip(TutorialType.Formations);
                ShowMenuTooltip(TutorialType.Formations);
                break;

            case UIState.Fleet:
                ShowInputTooltip(TutorialType.Fleet);
                break;

            default:
                HideTutorial(TutorialType.Formations, TutorialType.Fleet);
                break;
        }
    }

    public override void Generate()
    {
        inputContainer = new();
        inputContainer.AddToClassList("tutorial-input-container");
        inputContainer.pickingMode = PickingMode.Ignore;
        Root.Add(inputContainer);

        menuContainer = new();
        menuContainer.AddToClassList("tutorial-menu-container");
        menuContainer.pickingMode = PickingMode.Ignore;
        Root.Add(menuContainer);
    }

    private void CreateInputTooltips(List<Tooltip> _tooltips, bool shouldCreateHide)
    {
        inputContainer.Clear();

        VisualElement inputBackground = new();
        inputBackground.AddToClassList("tutorial-input-background");
        SetMargin(inputBackground, 0, 50, 40, 0);
        inputContainer.Add(inputBackground);

        foreach (Tooltip tooltip in _tooltips)
        {
            CreateInputTooltip(inputBackground, tooltip);
        }

        if (shouldCreateHide)
        {
            CreateInputTooltip(inputBackground, new Tooltip("Hide this", new TooltipInput("Z")));
        }
    }

    private void CreateInputTooltip(VisualElement _parent, Tooltip _tooltip)
    {
        VisualElement container = new();
        container.AddToClassList("tutorial-input-item");
        SetMargin(container, 0, 0, 10, 10);
        SetBorderRadius(container, 5);
        _parent.Add(container);

        for (int i = 0; i < _tooltip.Controls.Length; i++)
        {
            for (int j = 0; j < _tooltip.Controls[i].Keys.Length; j++)
            {
                Label inputLabel = new(_tooltip.Controls[i].Keys[j]);
                inputLabel.AddToClassList("tutorial-input-item-input");
                SetMargin(inputLabel, 0, 0, j == 0 ? 0 : 3, 3);
                SetPadding(inputLabel, 0, 0, 10, 10);
                SetBorderWidthRadius(inputLabel, 3, 5);
                SetFontSize(inputLabel, 20);
                container.Add(inputLabel);
            }

            if (i + 1 < _tooltip.Controls.Length)
            {
                Label alternative = new("or");
                alternative.AddToClassList("tutorial-input-item-description");
                SetFontSize(alternative, 18);
                container.Add(alternative);
            }
        }

        Label description = new(_tooltip.Description);
        description.AddToClassList("tutorial-input-item-description");
        SetFontSize(description, 18);
        container.Add(description);
    }

    public void CreateFormationsTutorial()
    {
        menuContainer.Clear();

        PlayerBoatController.Instance.AdmiralController.SetCommandForSubordinates(Command.Follow);

        Box background = new();
        background.AddToClassList("tutorial-menu-background");
        SetWidth(background, 500);
        SetBorderRadius(background, 10);
        menuContainer.Add(background);

        Label header = new("Formations");
        header.AddToClassList("tutorial-menu-header");
        SetMargin(header, 0, 30, 0, 0);
        SetFontSize(header, 40);
        background.Add(header);

        CreateMenuDescription(background, "This is the formation view, here you can get an overview of your fleet's formation.");
        CreateMenuDescription(background, "You can change what formation your fleet should use with the formation preset buttons that are placed right next to the follow command button.");
        CreateMenuDescription(background, "You can edit a formation preset by drag and dropping waypoints. Select a waypoint with the left mouse button, move the cursor, and release the left mouse button.");
        CreateMenuDescription(background, "You can cancel moving a waypoint with the right mouse button.");

        Button hide = new(() => OnHideMenu(TutorialType.FormationsMenu));
        hide.AddToClassList("main-button");
        hide.AddToClassList("tutorial-menu-button");
        hide.pickingMode = PickingMode.Position;
        SetMargin(hide, 30, 0, 0, 0);
        SetPadding(hide, 0, 0, 30, 30);
        SetFontSize(hide, 27);
        SetBorderWidthRadius(hide, 3, 7);
        hide.text = "Hide this";
        background.Add(hide);
    }

    private void OnHideMenu(TutorialType _type)
    {
        menuContainer.Clear();
        ignore.Add(_type);
    }

    private void CreateMenuDescription(VisualElement _parent, string _text)
    {
        Label label = new(_text);
        label.AddToClassList("tutorial-menu-description");
        SetFontSize(label, 18);
        _parent.Add(label);
    }

    public void ShowMenuTooltip(TutorialType _type)
    {
        if (ignore.Contains(_type)) return;

        switch (_type)
        {
            case TutorialType.Formations:
                CreateFormationsTutorial();
                break;

            default:
                Debug.LogError("Default");
                break;
        }
    }

    public void ShowInputTooltip(params TutorialType[] _types)
    {
        List<Tooltip> types = new();

        foreach (TutorialType type in _types)
        {
            if (!ignore.Contains(type))
            {
                types.AddRange(GetInputTooltipText(type));
            }
        }

        if (types.Count > 0)
        {
            current.Clear();
            current.AddRange(_types);
            CreateInputTooltips(types, !current.Contains(TutorialType.Fleet) && !current.Contains(TutorialType.Formations));
        }
    }

    private Tooltip[] GetInputTooltipText(TutorialType _type)
    {
        switch (_type)
        {
            case TutorialType.Player:
                return new Tooltip[] { new("Walk", new TooltipInput("W", "A", "S", "D")), new("Sprint", new TooltipInput("LShift")), new("Jump", new TooltipInput("SPACE")), new("Interact", new TooltipInput("E")), new("Objective", new TooltipInput("TAB")) };

            case TutorialType.Steering:
                return new Tooltip[] { new("Steer", new TooltipInput("A", "D")), new("Camera view", new TooltipInput("C")), new("Exit", new TooltipInput("E"), new TooltipInput("ESC")) };

            case TutorialType.Throttle:
                return new Tooltip[] { new("Throttle", new TooltipInput("A", "D")), new("Exit", new TooltipInput("E"), new TooltipInput("ESC")) };

            case TutorialType.Cannon:
                return new Tooltip[] { new("Aim", new TooltipInput("W", "A", "S", "D")), new("Fire", new TooltipInput("LMB")), new("Exit", new TooltipInput("E"), new TooltipInput("ESC")) };

            case TutorialType.Command:
                return new Tooltip[] { new("Show Current", new TooltipInput("TAB")), new("Fleet follow", new TooltipInput("1")), new("Fleet wait", new TooltipInput("2")), new("Fleet charge", new TooltipInput("3")), new("Formation view", new TooltipInput("4")) };

            case TutorialType.Formations:
                return new Tooltip[] { new("Move Camera", new TooltipInput("W", "A", "S", "D")), new("Zoom Camera", new TooltipInput("Scroll")), new("Fleet follow", new TooltipInput("1")), new("Fleet wait", new TooltipInput("2")), new("Fleet charge", new TooltipInput("3")), new("Exit", new TooltipInput("ESC", "4")) };

            case TutorialType.Fleet:
                return new Tooltip[] { new("Exit", new TooltipInput("ESC", "E")) };


            default:
                Debug.LogError("Default");
                return null;
        }
    }

    public void HideTutorial(params TutorialType[] _type)
    {
        if (current.Any((t) => _type.ToList().Contains(t)))
        {
            Hide();
        }
    }

    private void HideTooltip_performed(UnityEngine.InputSystem.InputAction.CallbackContext _obj)
    {
        if (current.Contains(TutorialType.Fleet) || current.Contains(TutorialType.Formations)) return;

        if (current.Count > 0)
        {
            ignore.AddRange(current);
            Hide();
        }
    }

    private void Hide()
    {
        current.Clear();
        menuContainer.Clear();
        inputContainer.Clear();
    }

    private struct Tooltip
    {
        public TooltipInput[] Controls;
        public string Description;

        public Tooltip(string description, params TooltipInput[] _controls)
        {
            Controls = _controls;
            Description = description;
        }
    }

    private struct TooltipInput
    {
        public string[] Keys;

        public TooltipInput(params string[] _keys)
        {
            Keys = _keys;
        }
    }
}

public enum TutorialType
{
    Player,
    Steering,
    Throttle,
    Cannon,
    Command,
    Fleet,
    Formations,

    FormationsMenu
}