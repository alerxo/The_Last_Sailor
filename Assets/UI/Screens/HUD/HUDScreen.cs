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

    #region HUD

    private Box admiralContainer;
    private Label admiralText;
    private VisualElement admiralIconContainer;

    private Box interactionBackground;
    private Label interactionText;

    #endregion

    #region Command

    public CommandObjectiveState CommandState { get; private set; } = CommandObjectiveState.Hidden;
    private float commandStateTimer = 0;
    private const float COMMAND_TIME_SHOWING = 2f;
    private const float COMMAND_TIME_FADING = 1f;

    private PlayerAdmiralController admiralController;
    [SerializeField] private Material followMaterial, waitMaterial, chargeMaterial;

    private VisualElement commandContainer;
    private readonly Dictionary<Command, Button> commandButtons = new();
    private readonly Dictionary<Formation, Button> formationButtons = new();

    #endregion

    #region Objective

    public CommandObjectiveState ObjectiveState { get; private set; } = CommandObjectiveState.Hidden;
    private float objectiveStateTimer = 0;
    private const float OBJECTIVE_TIME_SHOWING = 6f;
    private const float OBJECTIVE_TIME_FADING = 3f;

    private Box objectiveBackground;
    public readonly Dictionary<ObjectiveType, VisualElement> CurrentObjectives = new();
    public readonly List<ObjectiveType> CompletedObjectives = new();

    #endregion

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;

        UIManager.OnStateChanged += UIManager_OnStateChanged;
        InteractionCollider.OnInteractableChanged += InteractionCollider_OnInteractableChanged;
        CombatManager.OnAdmiralInCombatChanged += CombatManager_OnAdmiralInCombatChanged;
        FirstPersonController.OnPlayerStateChanged += FirstPersonController_OnPlayerStateChanged;
        ResourceManager.OnResourceAmountChanged += ResourceManager_OnResourceAmountChanged;
        FleetScreen.OnBoatBuilt += FleetScreen_OnBoatBuilt;
        CombatManager.OnBattleConcluded += CombatManager_OnBattleConcluded;
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
        ResourceManager.OnResourceAmountChanged -= ResourceManager_OnResourceAmountChanged;
        FleetScreen.OnBoatBuilt -= FleetScreen_OnBoatBuilt;
        CombatManager.OnBattleConcluded -= CombatManager_OnBattleConcluded;

        if (admiralController != null)
        {
            admiralController.OnCommandChanged -= AdmiralController_OnCommandChanged;
        }
    }

    private void Update()
    {
        RunCommandState();
        RunObjectiveState();
    }

    private void CombatManager_OnAdmiralInCombatChanged(Admiral _admiral)
    {
        TryCompleteFindObjective(_admiral);
        TryShowAdmiral(_admiral);
    }

    private void UIManager_OnStateChanged(UIState _state)
    {
        SetCommandContent();
        TryCompleteInspectObjective(_state);
        SetObjectiveBackgroundState();
    }

    private void AdmiralController_OnCommandChanged(Command _command)
    {
        SetCommandContent();
    }

    public override void Generate()
    {
        CreateHUD();
        CreateCommandContainer();
        CreateObjectiveContainer();

        SetObjectiveBackgroundState();
        SetCommandContainerState();
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

    private void TryShowAdmiral(Admiral _admiral)
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
            ShowObjective();
        }

        switch (CommandState)
        {
            case CommandObjectiveState.Visible:
                CommandShowingState();
                break;

            case CommandObjectiveState.Fading:
                CommandFadingState();
                break;

            case CommandObjectiveState.Hidden:
                CommandHiddenState();
                break;
        }
    }

    public void ShowCommand()
    {
        commandStateTimer = COMMAND_TIME_SHOWING;
        CommandState = CommandObjectiveState.Visible;
        SetCommandContainerState();
    }

    private void CommandShowingState()
    {
        if ((commandStateTimer -= Time.deltaTime) <= 0)
        {
            commandStateTimer = COMMAND_TIME_FADING;
            CommandState = CommandObjectiveState.Fading;
        }

        else if (commandContainer != null)
        {
            commandContainer.style.opacity = 1;
        }
    }

    private void CommandFadingState()
    {
        if ((commandStateTimer -= Time.deltaTime) <= 0)
        {
            CommandState = CommandObjectiveState.Hidden;
        }

        else if (commandContainer != null)
        {
            commandContainer.style.opacity = commandStateTimer / COMMAND_TIME_FADING;
        }
    }

    private void CommandHiddenState()
    {
        if (commandContainer != null)
        {
            commandContainer.style.opacity = 0;
        }
    }

    public void ForceHideCommand()
    {
        CommandState = CommandObjectiveState.Hidden;
    }

    private void SetCommandContainerState()
    {
        if (commandContainer != null)
        {
            commandContainer.visible = CompletedObjectives.Contains(ObjectiveType.InspectFleet) || CurrentObjectives.ContainsKey(ObjectiveType.InspectFleet);
        }
    }

    private void SetCommandContent()
    {
        if (commandContainer == null) return;

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

    private void CreateCommandContainer()
    {
        VisualElement container = new();
        container.AddToClassList("command-container");
        container.pickingMode = PickingMode.Ignore;
        Root.Add(container);

        CreateCommandContainer(container);
    }

    private void CreateCommandContainer(VisualElement container)
    {
        commandContainer = new();
        commandContainer.AddToClassList("command-button-container");
        SetMargin(commandContainer, 0, 50, 50, 0);
        commandContainer.pickingMode = PickingMode.Ignore;
        container.Add(commandContainer);

        VisualElement followContainer = new();
        followContainer.AddToClassList("command-follow-container");
        SetMargin(followContainer, 0, 30, 0, 0);
        commandContainer.Add(followContainer);

        CreateFormationButtonContainer(followContainer);

        CreateButton(followContainer, "1", $"{Command.Follow}", "Ships in fleet will follow the\nplayer in given formation", Command.Follow);
        CreateButton(commandContainer, "2", $"{Command.Wait}", "Ships in fleet will wait at\ncurrent position in given formation ", Command.Wait);
        CreateButton(commandContainer, "3", $"{Command.Charge}", "Ships in fleet will charge the\nclosest enemy", Command.Charge);

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

    #region Objective

    private void RunObjectiveState()
    {
        switch (ObjectiveState)
        {
            case CommandObjectiveState.Visible:
                ObjectiveShowingState();
                break;

            case CommandObjectiveState.Fading:
                ObjectiveFadingState();
                break;

            case CommandObjectiveState.Hidden:
                ObjectiveHiddenState();
                break;
        }
    }

    public void ShowObjective()
    {
        objectiveStateTimer = OBJECTIVE_TIME_SHOWING;
        ObjectiveState = CommandObjectiveState.Visible;
        SetObjectiveBackgroundState();
    }

    private void ObjectiveShowingState()
    {
        if ((objectiveStateTimer -= Time.deltaTime) <= 0)
        {
            objectiveStateTimer = OBJECTIVE_TIME_FADING;
            ObjectiveState = CommandObjectiveState.Fading;
        }

        else if (objectiveBackground != null)
        {
            objectiveBackground.style.opacity = 1;
        }
    }

    private void ObjectiveFadingState()
    {
        if ((objectiveStateTimer -= Time.deltaTime) <= 0)
        {
            ObjectiveState = CommandObjectiveState.Hidden;
        }

        else if (objectiveBackground != null)
        {
            objectiveBackground.style.opacity = objectiveStateTimer / OBJECTIVE_TIME_FADING;
        }
    }

    private void ObjectiveHiddenState()
    {
        if (objectiveBackground != null)
        {
            objectiveBackground.style.opacity = 0;
        }
    }

    private void CombatManager_OnBattleConcluded(BattleResult _state)
    {
        if (_state == BattleResult.Victory)
        {
            CompleteObjective(ObjectiveType.EliminateFirst);
        }

        else if (_state == BattleResult.BossDefeated)
        {
            CompleteObjective(ObjectiveType.FindAndEliminateRemaining);
        }
    }

    private void TryCompleteFindObjective(Admiral _admiral)
    {
        if (_admiral != null)
        {
            CompleteObjective(ObjectiveType.FindFirstEnemy);
        }
    }

    private void TryCompleteInspectObjective(UIState _state)
    {
        switch (_state)
        {
            case UIState.Fleet:
                CompleteObjective(ObjectiveType.InspectBoat);
                break;

            case UIState.Formation:
                CompleteObjective(ObjectiveType.InspectFleet);
                break;
        }
    }

    private void ResourceManager_OnResourceAmountChanged(float _amount)
    {
        if (_amount > 0)
        {
            AddObjective(ObjectiveType.InspectBoat);
        }
    }

    private void FleetScreen_OnBoatBuilt()
    {
        AddObjective(ObjectiveType.InspectFleet);
    }

    public void AddObjective(params ObjectiveType[] _types)
    {
        List<ObjectiveType> actual = new();

        foreach (ObjectiveType type in _types)
        {
            if (!CompletedObjectives.Contains(type) && !CurrentObjectives.ContainsKey(type))
            {
                actual.Add(type);
            }
        }

        if (actual.Count == 0) return;

        foreach (ObjectiveType type in actual)
        {
            CurrentObjectives.Add(type, CreateObjective(objectiveBackground, type));
        }

        ShowObjective();
    }

    public void CompleteObjective(params ObjectiveType[] _types)
    {
        List<ObjectiveType> actual = new();

        foreach (ObjectiveType type in _types)
        {
            if (CurrentObjectives.ContainsKey(type))
            {
                actual.Add(type);
            }
        }

        if (actual.Count == 0) return;

        foreach (ObjectiveType type in actual)
        {
            CurrentObjectives[type].RemoveFromHierarchy();
            CurrentObjectives.Remove(type);
            CompletedObjectives.Add(type);
            OnObjectiveCompleted(type);
        }
    }

    public void CreateObjectiveContainer()
    {
        VisualElement container = new();
        container.AddToClassList("objective-container");
        container.pickingMode = PickingMode.Ignore;
        Root.Add(container);

        objectiveBackground = new();
        objectiveBackground.AddToClassList("objective-background");
        SetMargin(objectiveBackground, 0, 0, 0, 50);
        SetBorderRadius(objectiveBackground, 10);
        container.Add(objectiveBackground);

        Label header = new("Objectives");
        header.AddToClassList("objective-header");
        SetFontSize(header, 30);
        objectiveBackground.Add(header);
    }

    private VisualElement CreateObjective(VisualElement _parent, ObjectiveType _type)
    {
        VisualElement container = new();
        container.AddToClassList("objective-item");
        _parent.Add(container);

        Box mark = new();
        mark.AddToClassList("objective-mark");
        SetSize(mark, 16, 16);
        SetMargin(mark, 0, 0, 0, 3);
        SetBorderWidth(mark, 3);
        container.Add(mark);

        Label label = new(GetObjectiveText(_type));
        label.AddToClassList("objective-label");
        SetFontSize(label, 22);
        container.Add(label);

        return container;
    }

    private string GetObjectiveText(ObjectiveType _type)
    {
        switch (_type)
        {
            case ObjectiveType.Engine:
                return "Start your engine";

            case ObjectiveType.Steer:
                return "Steer your ship";

            case ObjectiveType.FindFirstEnemy:
                return "Find the enemy";

            case ObjectiveType.EliminateFirst:
                return "Eliminate the enemy";

            case ObjectiveType.FindAndEliminateRemaining:
                return "Eliminate the remaining enemies";

            case ObjectiveType.InspectBoat:
                return "Inspect your ship in your office";

            case ObjectiveType.InspectFleet:
                return "Inspect your fleet inside the formation view";

            default:
                Debug.LogError("Defaulted");
                return "";
        }
    }

    private void OnObjectiveCompleted(ObjectiveType _type)
    {
        switch (_type)
        {
            case ObjectiveType.Engine:
                AddObjective(ObjectiveType.Steer);
                break;

            case ObjectiveType.Steer:
                AddObjective(ObjectiveType.FindFirstEnemy);
                CombatManager.Instance.EnableSpawning();
                break;

            case ObjectiveType.FindFirstEnemy:
                AddObjective(ObjectiveType.EliminateFirst);
                break;

            case ObjectiveType.EliminateFirst:
                AddObjective(ObjectiveType.FindAndEliminateRemaining);
                break;
        }
    }

    private void SetObjectiveBackgroundState()
    {
        if (objectiveBackground != null)
        {
            objectiveBackground.visible = UIManager.Instance.State == UIState.HUD && (CurrentObjectives.Count + CompletedObjectives.Count) > 0;
        }
    }

    #endregion
}

public enum CommandObjectiveState
{
    Visible,
    Fading,
    Hidden
}

public enum ObjectiveType
{
    Engine,
    Steer,
    FindFirstEnemy,
    EliminateFirst,
    FindAndEliminateRemaining,
    InspectBoat,
    InspectFleet
}