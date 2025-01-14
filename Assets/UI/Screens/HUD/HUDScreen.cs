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

    private IInteractable currentInteractable;
    private const float INTERACTION_SHADER_FADE = 0.15f;

    private Box admiralContainer;
    private Label admiralText;
    private VisualElement admiralIconContainer;

    private Box interactionBackground;
    private Label interactionText;

    private Coroutine currentInteractionButtonCoroutine;
    private Coroutine currentShaderCoroutine;

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
    private VisualElement formationContainer;
    private readonly Dictionary<Formation, Button> formationButtons = new();

    #endregion

    #region Objective

    public CommandObjectiveState ObjectiveState { get; private set; } = CommandObjectiveState.Hidden;

    private Box objectiveBackground;
    public readonly Dictionary<ObjectiveType, VisualElement> CurrentObjectives = new();
    public readonly List<ObjectiveType> CompletedObjectives = new();

    private readonly Queue<VisualElement> objectivesToAdd = new();
    private bool isAddingObjective = false;

    [SerializeField] private Texture2D checkIcon;

    private InputSystem_Actions input;

    #endregion

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;

        input = new();
        input.Player.Move.Enable();
        input.Player.Move.performed += Move_performed;

        UIManager.OnStateChanged += UIManager_OnStateChanged;
        InteractionCollider.OnInteractableChanged += InteractionCollider_OnInteractableChanged;
        CombatManager.OnAdmiralInCombatChanged += CombatManager_OnAdmiralInCombatChanged;
        FirstPersonController.OnPlayerStateChanged += FirstPersonController_OnPlayerStateChanged;
        FleetScreen.OnBoatBuilt += FleetScreen_OnBoatBuilt;
        CombatManager.OnBattleConcluded += CombatManager_OnBattleConcluded;
    }

    private void Start()
    {
        admiralController = PlayerBoatController.Instance.AdmiralController;
        admiralController.OnCommandChanged += AdmiralController_OnCommandChanged;
        admiralController.OnFormationChanged += AdmiralController_OnFormationChanged;
    }

    private void OnDestroy()
    {
        input.Disable();
        input.Player.Move.performed -= Move_performed;

        UIManager.OnStateChanged -= UIManager_OnStateChanged;
        InteractionCollider.OnInteractableChanged -= InteractionCollider_OnInteractableChanged;
        CombatManager.OnAdmiralInCombatChanged -= CombatManager_OnAdmiralInCombatChanged;
        FirstPersonController.OnPlayerStateChanged -= FirstPersonController_OnPlayerStateChanged;
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
        TryCompleteFindEnemy(_admiral);
        TryShowAdmiral(_admiral);
    }

    private void UIManager_OnStateChanged(UIState _state)
    {
        SetCommandContent();
        SetObjectiveBackgroundState();
    }

    private void AdmiralController_OnCommandChanged(Command _command)
    {
        SetCommandContent();
        TryCompleteChangeCommand();
    }

    private void AdmiralController_OnFormationChanged(Formation _formation)
    {
        SetCommandContent();
        TryCompleteChangeFormation();
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

        if (currentInteractionButtonCoroutine != null)
        {
            StopCoroutine(currentInteractionButtonCoroutine);
        }

        currentInteractionButtonCoroutine = _interactable == null ? StartCoroutine(HideInteractionButton()) : StartCoroutine(ShowInteractionButton());

        if (currentInteractable != null)
        {
            StartCoroutine(HideInteractionShaderTimer(currentInteractable.GetRenderers));
            currentInteractable = null;
        }

        if (_interactable != null)
        {
            currentInteractable = _interactable;

            StopCoroutine(HideInteractionShaderTimer(currentInteractable.GetRenderers));
            ShowInteractionShader(currentInteractable.GetRenderers);
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
        container.pickingMode = PickingMode.Ignore;
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

    private void ShowInteractionShader(Renderer[] _interactableRenderers)
    {
        for (int i = 0; i < _interactableRenderers.Length; i++)
        {
            if (_interactableRenderers[i] != null)
            {
                MaterialPropertyBlock propertyBlock = new();
                _interactableRenderers[i].GetPropertyBlock(propertyBlock);
                propertyBlock.SetFloat("_EffectBlend", 1);
                _interactableRenderers[i].SetPropertyBlock(propertyBlock);
            }

            else
            {
                Debug.LogWarning("Null interaction shader renderer");
            }
        }
    }

    private IEnumerator HideInteractionShaderTimer(Renderer[] _interactableRenderers)
    {
        float duration = 0;

        while ((duration += Time.deltaTime) < INTERACTION_SHADER_FADE)
        {
            float percentage = duration / INTERACTION_SHADER_FADE;

            for (int i = 0; i < _interactableRenderers.Length; i++)
            {
                if (_interactableRenderers[i] != null)
                {
                    MaterialPropertyBlock propertyBlock = new();
                    _interactableRenderers[i].GetPropertyBlock(propertyBlock);
                    propertyBlock.SetFloat("_EffectBlend", 1 - percentage);
                    _interactableRenderers[i].SetPropertyBlock(propertyBlock);
                }

                else
                {
                    Debug.LogWarning("Null interaction shader renderer");
                }
            }

            yield return null;
        }

        HideInteractionShader(_interactableRenderers);
    }

    private void HideInteractionShader(Renderer[] _interactableRenderers)
    {
        for (int i = 0; i < _interactableRenderers.Length; i++)
        {
            if (_interactableRenderers[i] != null)
            {
                MaterialPropertyBlock propertyBlock = new();
                _interactableRenderers[i].GetPropertyBlock(propertyBlock);
                propertyBlock.SetFloat("_EffectBlend", 0);
                _interactableRenderers[i].SetPropertyBlock(propertyBlock);
            }

            else
            {
                Debug.LogWarning("Null interaction shader renderer");
            }
        }
    }

    private IEnumerator ShowInteractionButton()
    {
        HideInteraction();
        float duration = 0;

        yield return new WaitForSeconds(1);

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
        if (UIManager.Instance.GetState() == UIState.Formation)
        {
            ShowCommand();
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
            commandContainer.visible = CompletedObjectives.Contains(ObjectiveType.BuildShip);
        }
    }

    private void SetCommandContent()
    {
        if (commandContainer == null) return;

        formationContainer.SetEnabled(UIManager.Instance.GetState() == UIState.Formation && PlayerBoatController.Instance.AdmiralController.Command != Command.Charge);

        foreach (Formation formation in formationButtons.Keys)
        {
            formationButtons[formation].SetEnabled(PlayerBoatController.Instance.AdmiralController.DefaultFormation != formation);
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
        SetMargin(container, 0, 50, 50, 0);
        container.pickingMode = PickingMode.Ignore;
        Root.Add(container);

        CreateCommandContainer(container);
    }

    private void CreateCommandContainer(VisualElement _container)
    {
        commandContainer = new();
        commandContainer.AddToClassList("command-button-container");
        SetMargin(commandContainer, 0, 0, 0, 0);
        commandContainer.pickingMode = PickingMode.Ignore;
        _container.Add(commandContainer);

        TutorialScreen.Instance.SetCommandContainer(commandContainer);

        VisualElement followWaitContainer = new();
        followWaitContainer.AddToClassList("command-follow-wait-container");
        SetMargin(followWaitContainer, 0, 30, 0, 0);
        commandContainer.Add(followWaitContainer);

        CreateFormationButtonContainer(followWaitContainer);

        VisualElement followWaitButtons = CreateButtonBackground(followWaitContainer);
        SetMargin(followWaitButtons, 0, 0, 0, 25);
        CreateButton(followWaitButtons, "1", $"{Command.Follow}", "Ships in fleet will follow the\nplayer in given formation", Command.Follow);
        CreateButton(followWaitButtons, "2", $"{Command.Wait}", "Ships in fleet will wait at\ncurrent position in given formation ", Command.Wait);

        VisualElement chargeButton = CreateButtonBackground(commandContainer);
        CreateButton(chargeButton, "3", $"{Command.Charge}", "Ships in fleet will charge the\nclosest enemy", Command.Charge);

        AdmiralController_OnCommandChanged(PlayerBoatController.Instance.AdmiralController.Command);
    }

    private VisualElement CreateButtonBackground(VisualElement _parent)
    {
        VisualElement followWaitButtons = new();
        followWaitButtons.AddToClassList("command-button-background");
        SetPadding(followWaitButtons, 20);
        SetBorderRadius(followWaitButtons, 10);
        _parent.Add(followWaitButtons);

        return followWaitButtons;
    }

    private void CreateButton(VisualElement _parent, string _input, string _name, string _description, Command _command)
    {
        Button button = new(() => admiralController.SetCommandForSubordinates(_command));
        button.AddToClassList("main-button");
        button.AddToClassList("command-button");
        button.pickingMode = PickingMode.Position;
        SetWidth(button, 300);
        SetMargin(button, 0, _command == Command.Follow ? 30 : 0, 0, 0);
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
        formationContainer = new();
        formationContainer.AddToClassList("command-formation-container");
        _parent.Add(formationContainer);

        CreateFormationButton(formationContainer, "Line [-]", Formation.Line);
        CreateFormationButton(formationContainer, "Spearhead [>]", Formation.Spearhead);
        CreateFormationButton(formationContainer, "Ring [O]", Formation.Ring);

        TutorialScreen.Instance.SetFormationsContainer(formationContainer);
    }

    private void CreateFormationButton(VisualElement _parent, string _text, Formation _formation)
    {
        Button button = new(() => PlayerBoatController.Instance.AdmiralController.SetDefaultFormation(_formation));
        button.AddToClassList("main-button");
        button.AddToClassList("command-formation-button");
        SetSize(button, 256, 48);
        SetBorderWidthRadius(button, 3, 7);
        SetFontSize(button, 30);
        button.pickingMode = PickingMode.Position;
        button.text = _text;
        button.SetEnabled(UIManager.Instance.GetState() == UIState.Formation && PlayerBoatController.Instance.AdmiralController.Command != Command.Charge);
        _parent.Add(button);

        formationButtons[_formation] = button;
    }

    #endregion

    #region Objective

    private void RunObjectiveState()
    {
        if (!isAddingObjective && objectivesToAdd.Count > 0 && UIManager.Instance.GetState() == UIState.HUD)
        {
            StartCoroutine(AddObjectiveAnimation(objectivesToAdd.Dequeue()));
        }

        switch (ObjectiveState)
        {
            case CommandObjectiveState.Visible:
                ObjectiveShowingState();
                break;

            case CommandObjectiveState.Hidden:
                ObjectiveHiddenState();
                break;
        }
    }

    public void ShowObjective()
    {
        ObjectiveState = CommandObjectiveState.Visible;
        SetObjectiveBackgroundState();
    }

    private void ObjectiveShowingState()
    {
        if (objectiveBackground != null && !objectiveBackground.enabledSelf)
        {
            objectiveBackground.SetEnabled(true);
        }

        if (CurrentObjectives.Count == 0 || (CurrentObjectives.Count == 1 && CurrentObjectives.ContainsKey(ObjectiveType.FindAndEliminateRemaining)))
        {
            ObjectiveState = CommandObjectiveState.Hidden;
        }
    }

    private void ObjectiveHiddenState()
    {
        if (objectiveBackground != null && objectiveBackground.enabledSelf)
        {
            objectiveBackground.SetEnabled(false);
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

    private void TryCompleteFindEnemy(Admiral _admiral)
    {
        if (_admiral != null)
        {
            CompleteObjective(ObjectiveType.FindFirstEnemy);
        }
    }

    private void TryCompleteChangeCommand()
    {
        if (CurrentObjectives.ContainsKey(ObjectiveType.ChangeCommand))
        {
            CompleteObjective(ObjectiveType.ChangeCommand);
        }
    }

    private void TryCompleteChangeFormation()
    {
        if (CurrentObjectives.ContainsKey(ObjectiveType.ChangeFormation))
        {
            CompleteObjective(ObjectiveType.ChangeFormation);
        }
    }

    private void Move_performed(InputAction.CallbackContext _obj)
    {
        input.Player.Move.Disable();
        StartCoroutine(AddEngineObjective());
    }

    private IEnumerator AddEngineObjective()
    {
        yield return new WaitForSeconds(1);

        AddObjective(ObjectiveType.Engine);
    }

    private void FleetScreen_OnBoatBuilt()
    {
        AddObjective(ObjectiveType.ChangeCommand, ObjectiveType.ChangeFormation);
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
        SetFontSize(header, 40);
        SetMargin(header, 0, 17, 0, 0);
        objectiveBackground.Add(header);
    }

    public void AddObjective(params ObjectiveType[] _types)
    {
        List<ObjectiveType> toAdd = new();

        foreach (ObjectiveType type in _types)
        {
            if (!CompletedObjectives.Contains(type) && !CurrentObjectives.ContainsKey(type))
            {
                toAdd.Add(type);
            }
        }

        if (toAdd.Count == 0) return;

        foreach (ObjectiveType type in toAdd)
        {
            VisualElement objective = CreateObjective(objectiveBackground, type);
            CurrentObjectives.Add(type, objective);
            objectivesToAdd.Enqueue(objective);
        }

        ShowObjective();
    }

    private VisualElement CreateObjective(VisualElement _parent, ObjectiveType _type)
    {
        VisualElement container = new();
        container.SetEnabled(false);
        container.AddToClassList("objective-item-first");
        container.AddToClassList("objective-item");
        SetWidth(container, 300);
        SetPadding(container, 7, 7, 7, 7);
        SetBorderRadius(container, 10);
        _parent.Add(container);

        Box mark = new();
        mark.AddToClassList("objective-mark");
        SetSize(mark, 26, 26);
        SetMargin(mark, 5, 0, 0, 4);
        SetBorderWidth(mark, 3);
        container.Add(mark);

        Label label = new(GetObjectiveText(_type));
        label.AddToClassList("objective-label");
        SetFontSize(label, 26);
        container.Add(label);

        return container;
    }

    public IEnumerator AddObjectiveAnimation(VisualElement _target)
    {
        isAddingObjective = true;

        yield return new WaitForSeconds(1f);

        _target.SetEnabled(true);

        yield return new WaitForSeconds(3);

        _target.RemoveFromClassList("objective-item-first");
        _target.AddToClassList("objective-item-second");

        _target.SetEnabled(false);

        isAddingObjective = false;
    }

    public void CompleteObjective(params ObjectiveType[] _types)
    {
        List<ObjectiveType> toRemove = new();

        foreach (ObjectiveType type in _types)
        {
            if (CurrentObjectives.ContainsKey(type))
            {
                toRemove.Add(type);
            }

            CompletedObjectives.Add(type);
        }

        foreach (ObjectiveType type in toRemove)
        {
            StartCoroutine(RemoveObjectiveAnimation(CurrentObjectives[type]));
            CurrentObjectives.Remove(type);
            OnObjectiveCompleted(type);
        }
    }

    private IEnumerator RemoveObjectiveAnimation(VisualElement _target)
    {
        yield return new WaitForSeconds(0.5f);

        Image image = new();
        image.AddToClassList("objective-mark-check");
        image.image = checkIcon;
        _target.ElementAt(0).Add(image);

        yield return AnimateScale(image, 0.3f, 0, 4);
        yield return AnimateScale(image, 0.2f, 4, 1);

        yield return new WaitForSeconds(4);

        yield return AnimateOpacity(_target, 3, 1, 0);

        yield return AnimateHeight(_target, 0.2f, _target.resolvedStyle.height, 0);

        _target.RemoveFromHierarchy();
    }

    private string GetObjectiveText(ObjectiveType _type)
    {
        switch (_type)
        {
            case ObjectiveType.Engine:
                return "Start your engine with the throttle";

            case ObjectiveType.Steer:
                return "Steer your ship with the wheel";

            case ObjectiveType.FindFirstEnemy:
                return "Find the enemy fleet";

            case ObjectiveType.EliminateFirst:
                return "Eliminate the enemy fleet";

            case ObjectiveType.ShootCannon:
                return "Shoot a cannon at the enemy ship";

            case ObjectiveType.FindAndEliminateRemaining:
                return "Eliminate the remaining enemy fleets";

            case ObjectiveType.RepairShip:
                return "Repair your fleet at your desk";

            case ObjectiveType.UpgradeShip:
                return "Upgrade a ship at your desk";

            case ObjectiveType.BuildShip:
                return "Build a new ship at your desk";

            case ObjectiveType.ChangeCommand:
                return "Give your fleet a command";

            case ObjectiveType.ChangeFormation:
                return "Change your fleet's formation";

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
                AddObjective(ObjectiveType.ShootCannon);
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
            objectiveBackground.visible = UIManager.Instance.GetState() == UIState.HUD && (CurrentObjectives.Count + CompletedObjectives.Count) > 0;
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
    ShootCannon,
    EliminateFirst,
    FindAndEliminateRemaining,

    RepairShip,
    UpgradeShip,
    BuildShip,

    ChangeCommand,
    ChangeFormation,
}