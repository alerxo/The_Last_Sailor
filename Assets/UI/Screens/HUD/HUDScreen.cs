using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class HUDScreen : UIScreen
{
    public static HUDScreen Instance { get; private set; }

    private const float ADMIRAL_BACKGROUND_WIDTH = 1000f;
    private const float INTERACTION_BUTTON_SIZE = 100f;
    private const float INTERACTION_BUTTON_FONT_SIZE = 60;
    private const float INTERACTION_ANIMATION_DURATION = 0.1f;

    [SerializeField] private InputActionReference interactionAsset;
    [SerializeField] private Texture2D enemyIcon;

    protected override List<UIState> ActiveStates => new() { UIState.HUD, UIState.Formation };

    private Box admiralContainer;
    private Label admiralText;
    private VisualElement admiralIconContainer;

    private Box interactionBackground;
    private Label interactionText;

    public CommandScreenState State { get; private set; } = CommandScreenState.Hidden;
    private float stateTimer = 0;
    private const float TIME_SHOWING = 2f;
    private const float TIME_FADING = 1f;

    private PlayerAdmiralController admiralController;
    [SerializeField] private Material followMaterial, waitMaterial, chargeMaterial;

    private VisualElement buttonContainer;
    private readonly Dictionary<Command, Button> commandButtons = new();
    private readonly Dictionary<Formation, Button> formationButtons = new();

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;

        UIManager.OnStateChanged += UIManager_OnStateChanged;
        InteractionCollider.OnInteractableChanged += InteractionCollider_OnInteractableChanged;
        CombatManager.OnAdmiralInCombatChanged += CombatManager_OnAdmiralInCombatChanged;
        FirstPersonController.OnPlayerStateChanged += FirstPersonController_OnPlayerStateChanged;
    }

    private void Start()
    {
        admiralController = PlayerBoatController.Instance.AdmiralController;
        admiralController.OnCommandChanged += AdmiralController_OnCommandChanged;
    }

    private void OnDestroy()
    {
        UIManager.OnStateChanged -= UIManager_OnStateChanged;
        InteractionCollider.OnInteractableChanged -= InteractionCollider_OnInteractableChanged;
        CombatManager.OnAdmiralInCombatChanged -= CombatManager_OnAdmiralInCombatChanged;
        FirstPersonController.OnPlayerStateChanged -= FirstPersonController_OnPlayerStateChanged;

        if (admiralController != null)
        {
            admiralController.OnCommandChanged -= AdmiralController_OnCommandChanged;
        }
    }

    private void Update()
    {
        RunCommandState();
    }

    public override void Generate()
    {
        CreateHUD();
        CreateCommand();
        CreateObjective();
    }

    #region HUD

    private void InteractionCollider_OnInteractableChanged(IInteractable _interactable)
    {
        if (interactionBackground == null) return;

        if (_interactable == null)
        {
            StopCoroutine(ShowInteractionButton());
            StopCoroutine(HideInteractionButton());
            StartCoroutine(HideInteractionButton());
        }

        else
        {
            StopCoroutine(ShowInteractionButton());
            StopCoroutine(HideInteractionButton());
            StartCoroutine(ShowInteractionButton());
        }
    }

    private void CombatManager_OnAdmiralInCombatChanged(Admiral _admiral)
    {
        if (admiralContainer == null) return;

        if (_admiral != null)
        {
            admiralText.text = $"{(CombatManager.Instance.Round <= CombatManager.ENEMY_FLEET_SIZES.Length ? $"({CombatManager.Instance.Round}/{CombatManager.ENEMY_FLEET_SIZES.Length})   " : "")}{_admiral.Name}";

            admiralIconContainer.Clear();

            for (int i = 0; i < CombatManager.Instance.GetDifficulty(); i++)
            {
                CreateAdmiralIcon();
            }

            StopCoroutine(ShowAdmiralContainer());
            StartCoroutine(ShowAdmiralContainer());
        }
    }

    private void FirstPersonController_OnPlayerStateChanged(PlayerState _state)
    {
        switch (_state)
        {
            case PlayerState.FirstPerson:
                interactionBackground.style.display = DisplayStyle.Flex;
                break;

            case PlayerState.SteeringWheel:
            case PlayerState.Throttle:
            case PlayerState.Cannon:
                interactionBackground.style.display = DisplayStyle.None;
                break;
        }
    }

    private VisualElement CreateHUD()
    {
        VisualElement container = new();
        container.AddToClassList("hud-container");
        Root.Add(container);

        CreateAdmiral(container);
        CreateInteraction(container);

        HideAdmiral();
        HideInteraction();
        return container;
    }

    private void CreateAdmiral(VisualElement _parent)
    {
        admiralContainer = new();
        admiralContainer.AddToClassList("hud-admiral-container");
        SetMargin(admiralContainer, 50, 0, 0, 0);
        SetHeight(admiralContainer, 85);
        SetBorderRadius(admiralContainer, 10);
        _parent.Add(admiralContainer);

        admiralText = new("Admiral");
        admiralText.AddToClassList("hud-admiral-text");
        SetMargin(admiralText, 0, 0, 0, 20);
        SetPadding(admiralText, 10);
        SetFontSize(admiralText, 50);
        admiralContainer.Add(admiralText);

        admiralIconContainer = new();
        admiralIconContainer.AddToClassList("hud-admiral-icon-container");
        admiralContainer.Add(admiralIconContainer);
    }

    private void CreateAdmiralIcon()
    {
        Image image = new();
        image.AddToClassList("hud-admiral-icon");
        SetSize(image, 64, 64);
        image.image = enemyIcon;
        admiralIconContainer.Add(image);
    }

    private IEnumerator ShowAdmiralContainer()
    {
        const float BORDER_WIDTH = 5f;
        const float BORDER_DURATION = 0.1f;
        const float BACKGROUND_DURATION = 0.5f;

        HideAdmiral();

        yield return AnimateBorderWidth(admiralContainer, BORDER_DURATION, 0, 5f);
        yield return AnimateWidth(admiralContainer, BACKGROUND_DURATION, 0, ADMIRAL_BACKGROUND_WIDTH);

        yield return new WaitForSeconds(10f);

        yield return AnimateWidth(admiralContainer, BACKGROUND_DURATION, ADMIRAL_BACKGROUND_WIDTH, 0);
        yield return AnimateBorderWidth(admiralContainer, BORDER_DURATION, BORDER_WIDTH, 0);
    }

    private void HideAdmiral()
    {
        SetWidth(admiralContainer, 0);
        SetBorderWidth(admiralContainer, 0);
    }

    private void CreateInteraction(VisualElement _parent)
    {
        VisualElement interactionContainer = new();
        interactionContainer.AddToClassList("hud-interaction-container");
        SetMargin(interactionContainer, 0, 100, 0, 0);
        SetSize(interactionContainer, INTERACTION_BUTTON_SIZE, INTERACTION_BUTTON_SIZE);
        _parent.Add(interactionContainer);

        interactionBackground = new();
        interactionBackground.AddToClassList("hud-interaction-background");
        SetSize(interactionBackground, 0, 0);
        SetBorderWidth(interactionBackground, 0);
        interactionContainer.Add(interactionBackground);

        interactionText = new(InputControlPath.ToHumanReadableString(interactionAsset.action.bindings[0].effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice));
        interactionText.AddToClassList("hud-interaction-text");
        SetFontSize(interactionText, 0);
        interactionBackground.Add(interactionText);
    }

    private IEnumerator ShowInteractionButton()
    {
        HideInteraction();

        float duration = 0;

        while ((duration += Time.deltaTime) < INTERACTION_ANIMATION_DURATION)
        {
            float percentage = duration / INTERACTION_ANIMATION_DURATION;
            float size = Mathf.Lerp(0, INTERACTION_BUTTON_SIZE, percentage);
            SetSize(interactionBackground, size, size);
            SetFontSize(interactionText, Mathf.Lerp(0, INTERACTION_BUTTON_FONT_SIZE, percentage));

            yield return null;
        }

        ShowInteraction();
    }

    private IEnumerator HideInteractionButton()
    {
        ShowInteraction();

        float duration = 0;

        while ((duration += Time.deltaTime) < INTERACTION_ANIMATION_DURATION)
        {
            float percentage = duration / INTERACTION_ANIMATION_DURATION;
            float size = Mathf.Lerp(INTERACTION_BUTTON_SIZE, 0, percentage);
            SetSize(interactionBackground, size, size);
            SetFontSize(interactionText, Mathf.Lerp(INTERACTION_BUTTON_FONT_SIZE, 0, percentage));

            yield return null;
        }

        HideInteraction();
    }

    private void ShowInteraction()
    {
        SetSize(interactionBackground, INTERACTION_BUTTON_SIZE, INTERACTION_BUTTON_SIZE);
        SetFontSize(interactionText, INTERACTION_BUTTON_FONT_SIZE);
    }

    private void HideInteraction()
    {
        SetSize(interactionBackground, 0, 0);
        SetFontSize(interactionText, 0);
    }

    #endregion

    #region Command

    private void RunCommandState()
    {
        if (UIManager.Instance.State == UIState.Formation)
        {
            ShowCommand();
        }

        switch (State)
        {
            case CommandScreenState.Visible:
                CommandShowingState();
                break;

            case CommandScreenState.Fading:
                CommandFadingState();
                break;

            case CommandScreenState.Hidden:
                CommandHiddenState();
                break;
        }
    }

    public void ShowCommand()
    {
        stateTimer = TIME_SHOWING;
        State = CommandScreenState.Visible;
    }

    private void CommandShowingState()
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

    private void CommandFadingState()
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

    private void CommandHiddenState()
    {
        buttonContainer.style.opacity = 0;
    }

    public void ForceHideCommand()
    {
        State = CommandScreenState.Hidden;
    }

    private void UIManager_OnStateChanged(UIState _state)
    {
        SetCommandContent();
    }

    private void AdmiralController_OnCommandChanged(Command _command)
    {
        SetCommandContent();
    }

    private void SetCommandContent()
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
                return followMaterial;

            case Command.Wait:
                return waitMaterial;

            case Command.Charge:
                return chargeMaterial;

            default:
                Debug.LogError("Defaulted");
                return null;
        }
    }

    private void CreateCommand()
    {
        VisualElement container = new();
        container.AddToClassList("command-container");
        container.pickingMode = PickingMode.Ignore;
        Root.Add(container);

        CreateCommandContainer(container);

        CommandHiddenState();
    }

    private void CreateCommandContainer(VisualElement container)
    {
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

    #endregion

    #region Objetive

    public void CreateObjective()
    {
        VisualElement container = new();
        container.AddToClassList("objective-container");
        container.pickingMode = PickingMode.Ignore;
        Root.Add(container);
    }

    #endregion
}

public enum CommandScreenState
{
    Visible,
    Fading,
    Hidden
}