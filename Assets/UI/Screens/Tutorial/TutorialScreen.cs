using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

public class TutorialScreen : UIScreen
{
    public static TutorialScreen Instance { get; private set; }

    protected override List<UIState> ActiveStates => new() { UIState.HUD, UIState.Fleet, UIState.Formation };

    private readonly List<TutorialType> ignoreTypes = new();
    private readonly List<TutorialType> newTypes = new() { TutorialType.Player, TutorialType.Steering, TutorialType.Throttle, TutorialType.Cannon, TutorialType.Command };
    private readonly List<TutorialType> currentTypes = new();

    private bool isCheckingIfCompletedInput = false;
    private readonly List<string> completedInput = new();
    private readonly Dictionary<KeyCode, TooltipControlScheme> currentInput = new();

    private VisualElement inputContainer;
    private VisualElement menuContainer;

    private Box tutorialBorder;
    private VisualElement commandContainer, formationContainer;

    private InputSystem_Actions input;

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;

        input = new();
        input.Player.HideTooltip.performed += HideTooltip_performed;
        input.Player.HideTooltip.Enable();

        UIManager.OnStateChanged += UIManager_OnStateChanged;
    }

    private void OnDestroy()
    {
        input.Player.HideTooltip.performed -= HideTooltip_performed;
        input.Player.Disable();

        UIManager.OnStateChanged -= UIManager_OnStateChanged;
    }

    private void Update()
    {
        if (isCheckingIfCompletedInput)
        {
            CheckIfCompletedInput();
        }
    }

    private void UIManager_OnStateChanged(UIState _state)
    {
        switch (_state)
        {
            case UIState.Formation:
                ShowInputTooltip(TutorialType.Formations);
                ShowMenuTooltip(TutorialType.FormationsMenu);
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

    #region Menu

    public void ShowMenuTooltip(TutorialType _type)
    {
        if (ignoreTypes.Contains(_type)) return;

        switch (_type)
        {
            case TutorialType.FormationsMenu:
                CreateFirstFormationsTutorial();
                break;

            default:
                Debug.LogError("Default");
                break;
        }
    }

    public void CreateFirstFormationsTutorial()
    {
        PlayerBoatController.Instance.AdmiralController.SetCommandForSubordinates(Command.Follow);

        CreateFormationTutorial("Formation view", "This is the formation view, here you can get an overview of your fleet and give commmands and formations.", CreateSecondFormationsTutorial);
    }

    public void CreateSecondFormationsTutorial()
    {
        CreateFormationTutorial("Fleet Commands",
            "You can change what command your fleet should follow with the command buttons.",
            CreateThirdFormationsTutorial,
            commandContainer);
    }

    public void CreateThirdFormationsTutorial()
    {
        CreateFormationTutorial("Fleet Formations",
            "You can change what formation your fleet should use by with formation preset buttons.",
            CreateFourthFormationsTutorial,
            formationContainer);
    }

    public void CreateFourthFormationsTutorial()
    {
        CreateFormationTutorial("Fleet Waypoints",
            "You can edit a formation preset by drag and dropping ship waypoints. Select a waypoint with the left mouse button, move the cursor, and release the left mouse button. You can cancel moving a waypoint with the right mouse button.",
            () => OnHideMenu(TutorialType.FormationsMenu));
    }

    private void CreateFormationTutorial(string _header, string _description, Action _onContinue, VisualElement _tutorialBorder = null)
    {
        RemoveTutorialBorder();
        menuContainer.Clear();

        Box background = new();
        background.AddToClassList("tutorial-menu-background");
        SetWidth(background, 500);
        SetBorderRadius(background, 10);
        menuContainer.Add(background);

        Label header = new(_header);
        header.AddToClassList("tutorial-menu-header");
        SetMargin(header, 0, 30, 0, 0);
        SetFontSize(header, 40);
        background.Add(header);

        CreateMenuDescription(background, _description);

        if (_tutorialBorder != null)
        {
            CreateTutorialBorder(_tutorialBorder);
        }

        Button continueButton = new(_onContinue);
        continueButton.AddToClassList("main-button");
        continueButton.AddToClassList("tutorial-menu-button");
        continueButton.pickingMode = PickingMode.Position;
        SetMargin(continueButton, 30, 0, 0, 0);
        SetPadding(continueButton, 0, 0, 30, 30);
        SetFontSize(continueButton, 27);
        SetBorderWidthRadius(continueButton, 3, 7);
        continueButton.text = "Continue";
        continueButton.SetEnabled(false);
        background.Add(continueButton);

        StartCoroutine(EnableContinueButton(continueButton));
    }

    private IEnumerator EnableContinueButton(Button _button)
    {
        yield return new WaitForSeconds(3);
        _button.SetEnabled(true);
    }

    private void OnHideMenu(TutorialType _type)
    {
        menuContainer.Clear();
        ignoreTypes.Add(_type);
    }

    private void CreateMenuDescription(VisualElement _parent, string _text)
    {
        Label label = new(_text.Replace(". ", ".\n\n"));
        label.AddToClassList("tutorial-menu-description");
        SetFontSize(label, 22);
        _parent.Add(label);
    }

    private void CreateTutorialBorder(VisualElement _parent)
    {
        RemoveTutorialBorder();

        VisualElement container = new();
        container.AddToClassList("tutorial-menu-border-container");
        _parent.Add(container);

        tutorialBorder = new();
        tutorialBorder.AddToClassList("tutorial-menu-border");
        SetBorderWidthRadius(tutorialBorder, 10, 20);
        container.Add(tutorialBorder);
    }

    private void RemoveTutorialBorder()
    {
        if (tutorialBorder != null)
        {
            tutorialBorder.parent.RemoveFromHierarchy();
            tutorialBorder = null;
        }
    }

    public void SetCommandContainer(VisualElement _command)
    {
        commandContainer = _command;
    }

    public void SetFormationsContainer(VisualElement _formation)
    {
        formationContainer = _formation;
    }

    #endregion

    #region Input

    public void ShowInputTooltip(params TutorialType[] _types)
    {
        List<Tooltip> types = new();

        foreach (TutorialType type in _types)
        {
            if (!ignoreTypes.Contains(type))
            {
                types.Add(new Tooltip(type, GetTooltipControls(type)));
            }
        }

        if (types.Count > 0)
        {
            currentTypes.Clear();
            currentTypes.AddRange(_types);
            CreateInputTooltips(types);
        }
    }

    private void CreateInputTooltips(List<Tooltip> _tooltips)
    {
        CheckIfCompletedInput();
        inputContainer.Clear();
        currentInput.Clear();

        VisualElement inputBackground = new();
        inputBackground.AddToClassList("tutorial-input-background");
        SetMargin(inputBackground, 0, 50, 40, 0);
        inputContainer.Add(inputBackground);

        bool isAnimating = false;

        foreach (Tooltip tooltip in _tooltips)
        {
            foreach (TooltipControlScheme controlScheme in tooltip.Controls)
            {
                TooltipControlScheme current = controlScheme;
                current.Type = tooltip.Type;
                current.visualElement = CreateInputControl(inputBackground, current);

                foreach (TooltipInput input in current.Controls)
                {
                    foreach (TooltipKey key in input.Keys)
                    {
                        currentInput[key.Key] = current;
                    }
                }

                if (completedInput.Contains(current.GetID()))
                {
                    current.visualElement.style.opacity = 0.5f;
                }

                if (newTypes.Contains(tooltip.Type))
                {
                    StartCoroutine(AnimateInputControl(current.visualElement));
                }
            }

            if (newTypes.Contains(tooltip.Type))
            {
                newTypes.Remove(tooltip.Type);
                isAnimating = true;
            }
        }

        if (!currentTypes.Contains(TutorialType.Fleet) && !currentTypes.Contains(TutorialType.Formations))
        {
            VisualElement control = CreateInputControl(inputBackground, new TooltipControlScheme("Hide Input UI", new TooltipInput(new TooltipKey(KeyCode.Z, "Z"))));

            if (isAnimating)
            {
                StartCoroutine(AnimateInputControl(control));
            }
        }

        StartCoroutine(PauseCheckingInput());
    }

    private IEnumerator PauseCheckingInput()
    {
        isCheckingIfCompletedInput = false;

        yield return new WaitForSeconds(0.1f);

        isCheckingIfCompletedInput = true;
    }

    private VisualElement CreateInputControl(VisualElement _parent, TooltipControlScheme _tooltip)
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
                Label inputLabel = new(_tooltip.Controls[i].Keys[j].Name);
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

        return container;
    }

    private IEnumerator AnimateInputControl(VisualElement _target)
    {
        yield return new WaitForSeconds(1f);

        _target.SetEnabled(false);

        yield return new WaitForSeconds(0.25f);

        _target.SetEnabled(true);
    }

    private TooltipControlScheme[] GetTooltipControls(TutorialType _type)
    {
        switch (_type)
        {
            case TutorialType.Player:
                return new TooltipControlScheme[] {
                    new("Walk", new TooltipInput(new TooltipKey(KeyCode.W, "W"), new TooltipKey(KeyCode.A, "A"), new TooltipKey(KeyCode.S, "S"), new TooltipKey(KeyCode.D, "D"))),
                    new("Sprint", new TooltipInput(new TooltipKey(KeyCode.LeftShift, "LShift"))),
                    new("Jump", new TooltipInput(new TooltipKey(KeyCode.Space, "Space"))),
                    new("Interact", new TooltipInput(new TooltipKey(KeyCode.E, "E"))) };

            case TutorialType.Steering:
                return new TooltipControlScheme[] {
                    new("Steer", new TooltipInput(new TooltipKey(KeyCode.A, "A"), new TooltipKey(KeyCode.D, "D"))),
                    new("Camera view", new TooltipInput(new TooltipKey(KeyCode.C, "C"))),
                    new("Exit", new TooltipInput(new TooltipKey(KeyCode.E, "E")),
                    new TooltipInput(new TooltipKey(KeyCode.Escape, "Esc"))) };

            case TutorialType.Throttle:
                return new TooltipControlScheme[] {
                    new("Throttle", new TooltipInput(new TooltipKey(KeyCode.A, "A"), new TooltipKey(KeyCode.D, "D"))),
                    new("Exit", new TooltipInput(new TooltipKey(KeyCode.E, "E"), new TooltipKey(KeyCode.Escape, "Esc"))) };

            case TutorialType.Cannon:
                return new TooltipControlScheme[] {
                    new("Aim", new TooltipInput(new TooltipKey(KeyCode.W, "W"), new TooltipKey(KeyCode.A, "A"), new TooltipKey(KeyCode.S, "S"), new TooltipKey(KeyCode.D, "D"))),
                    new("Fire", new TooltipInput(new TooltipKey(KeyCode.Mouse0, "LMB"))),
                    new("Exit", new TooltipInput(new TooltipKey(KeyCode.E, "E"), new TooltipKey(KeyCode.Escape, "Esc"))) };

            case TutorialType.Command:
                return new TooltipControlScheme[] {
                    new("Fleet follow", new TooltipInput(new TooltipKey(KeyCode.Alpha1, "1"))),
                    new("Fleet wait", new TooltipInput(new TooltipKey(KeyCode.Alpha2, "2"))),
                    new("Fleet charge", new TooltipInput(new TooltipKey(KeyCode.Alpha3, "3"))),
                    new("Formation view", new TooltipInput(new TooltipKey(KeyCode.Alpha4, "4"))) };

            case TutorialType.Formations:
                return new TooltipControlScheme[] {
                    new("Move Camera", new TooltipInput(new TooltipKey("W"), new TooltipKey("A"), new TooltipKey("S"), new TooltipKey("D"))),
                    new("Zoom Camera", new TooltipInput(new TooltipKey("Scroll"))),
                    new("Fleet follow", new TooltipInput(new TooltipKey("1"))),
                    new("Fleet wait", new TooltipInput(new TooltipKey("2"))),
                    new("Fleet charge", new TooltipInput(new TooltipKey("3"))),
                    new("Exit", new TooltipInput(new TooltipKey("4"), new TooltipKey("Esc"))) };

            case TutorialType.Fleet:
                return new TooltipControlScheme[] {
                    new("Exit", new TooltipInput(new TooltipKey("E"), new TooltipKey("Esc"))) };

            default:
                Debug.LogError("Default");
                return null;
        }
    }

    #endregion

    private void CheckIfCompletedInput()
    {
        foreach (KeyValuePair<KeyCode, TooltipControlScheme> controlScheme in currentInput)
        {
            if (Input.GetKeyDown(controlScheme.Key))
            {
                completedInput.Add(controlScheme.Value.GetID());
                controlScheme.Value.visualElement.style.opacity = 0.5f;
            }
        }
    }

    public void HideTutorial(params TutorialType[] _type)
    {
        if (currentTypes.Any((t) => _type.ToList().Contains(t)))
        {
            Hide();
        }
    }

    private void HideTooltip_performed(UnityEngine.InputSystem.InputAction.CallbackContext _obj)
    {
        if (currentTypes.Contains(TutorialType.Fleet) || currentTypes.Contains(TutorialType.Formations)) return;

        if (currentTypes.Count > 0)
        {
            ignoreTypes.AddRange(currentTypes);
            Hide();
        }
    }

    private void Hide()
    {
        CheckIfCompletedInput();

        currentTypes.Clear();
        menuContainer.Clear();
        inputContainer.Clear();
        currentInput.Clear();

        RemoveTutorialBorder();
    }

    private struct Tooltip
    {
        public TutorialType Type;
        public TooltipControlScheme[] Controls;

        public Tooltip(TutorialType _type, TooltipControlScheme[] _controls)
        {
            Type = _type;
            Controls = _controls;
        }
    }

    private struct TooltipControlScheme
    {
        public TooltipInput[] Controls;
        public TutorialType Type;
        public string Description;
        public VisualElement visualElement;

        public TooltipControlScheme(string description, params TooltipInput[] _controls)
        {
            Controls = _controls;
            Description = description;
            visualElement = null;
            Type = TutorialType.None;
        }

        public readonly string GetID()
        {
            return $"{Type}{Description}";
        }
    }

    private struct TooltipInput
    {
        public TooltipKey[] Keys;

        public TooltipInput(params TooltipKey[] _keys)
        {
            Keys = _keys;
        }
    }

    private struct TooltipKey
    {
        public KeyCode Key;
        public string Name;

        public TooltipKey(KeyCode _key, string _name)
        {
            Key = _key;
            Name = _name;
        }

        public TooltipKey(string _name)
        {
            Key = KeyCode.None;
            Name = _name;
        }
    }
}

public enum TutorialType
{
    None,

    Player,
    Steering,
    Throttle,
    Cannon,
    Command,
    Fleet,
    Formations,

    FormationsMenu
}