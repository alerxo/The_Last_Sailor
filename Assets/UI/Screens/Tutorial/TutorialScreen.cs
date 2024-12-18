using System.Collections.Generic;
using System.Linq;
using Unity.AppUI.UI;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

public class TutorialScreen : UIScreen
{
    public static TutorialScreen Instance { get; private set; }

    protected override List<UIState> ActiveStates => new() { UIState.HUD, UIState.Fleet, UIState.Formation };

    private readonly List<TutorialType> ignore = new();
    private readonly List<TutorialType> current = new();

    private VisualElement container;

    private InputSystem_Actions input;

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;

        input = new();
        input.Player.HideTooltip.performed += HideTooltip_performed;
        input.Player.Enable();
    }

    private void OnDestroy()
    {
        input.Player.HideTooltip.performed -= HideTooltip_performed;
        input.Player.Disable();
    }

    public override void Generate()
    {
        container = new();
        container.AddToClassList("tutorial-container");
        container.pickingMode = PickingMode.Ignore;
        Root.Add(container);
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
            CreateInputTooltips(types);
        }
    }

    private Tooltip[] GetInputTooltipText(TutorialType _type)
    {
        switch (_type)
        {
            case TutorialType.Player:
                return new Tooltip[] { new("Walk", new TooltipInput("W", "A", "S", "D")), new("Sprint", new TooltipInput("LShift")), new("Jump", new TooltipInput("SPACE")), new("Interact", new TooltipInput("E")) };

            case TutorialType.Steering:
                return new Tooltip[] { new("Steer", new TooltipInput("A", "D")), new("Camera view", new TooltipInput("C")), new("Exit", new TooltipInput("E"), new TooltipInput("ESC")) };

            case TutorialType.Throttle:
                return new Tooltip[] { new("Throttle", new TooltipInput("A", "D")), new("Exit", new TooltipInput("E"), new TooltipInput("ESC")) };

            case TutorialType.Cannon:
                return new Tooltip[] { new("Aim", new TooltipInput("W", "A", "S", "D")), new("Fire", new TooltipInput("LMB")), new("Exit", new TooltipInput("E"), new TooltipInput("ESC")) };

            case TutorialType.Command:
                return new Tooltip[] { new("Fleet follow", new TooltipInput("1")), new("Fleet wait", new TooltipInput("2")), new("Fleet charge", new TooltipInput("3")), new("Formation view", new TooltipInput("4")) };

            default:
                Debug.LogError("Default");
                return null;
        }
    }

    public void ShowMenuTooltip(TutorialType _type)
    {
        if (ignore.Contains(_type)) return;

        switch (_type)
        {
            case TutorialType.Fleet:
                break;

            case TutorialType.Formations:
                break;

            default:
                Debug.LogError("Default");
                break;
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
        if (current.Count > 0)
        {
            ignore.AddRange(current);
            Hide();
        }
    }

    private void Hide()
    {
        current.Clear();
        container.Clear();
    }

    private void CreateInputTooltips(List<Tooltip> _tooltips)
    {
        container.Clear();

        VisualElement inputContainer = new();
        inputContainer.AddToClassList("tutorial-input-container");
        SetMargin(inputContainer, 0, 50, 40, 0);
        container.Add(inputContainer);

        foreach (Tooltip tooltip in _tooltips)
        {
            CreateInputTooltip(inputContainer, tooltip);
        }

        CreateInputTooltip(inputContainer, new Tooltip("Hide this", new TooltipInput("Z")));
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
    Formations
}