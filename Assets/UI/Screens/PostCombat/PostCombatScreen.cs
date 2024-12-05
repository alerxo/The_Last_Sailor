using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PostCombatScreen : UIScreen
{
    public static event UnityAction OnBoatScrapped;
    public static event UnityAction OnBoatSeized;
    public static event UnityAction OnBoatRepaired;

    public static PostCombatScreen Instance { get; private set; }

    protected override List<UIState> ActiveStates => new() { UIState.PostCombat };

    private const float BACKGROUND_WIDTH = 1200f;
    private Box background;

    private const float HEADER_HEIGHT = 130f;
    private VisualElement headerContainer;

    private const float RESOURCE_HEIGHT = 50f;
    private VisualElement resourceContainer;
    private Label resourceCount;

    private const float RESULTS_HEIGHT = 500f;
    private VisualElement battleResultsContainer;
    private Box battleResultsBackground;

    private const float NAVIGATION_HEIGHT = 140f;
    private VisualElement navigationButtonContainer;
    private readonly List<VisualElement> opacityElements = new();

    private readonly List<PostCombatButton> postCombatButtons = new();
    private readonly List<Action> scrapActions = new();

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;

        ResourceManager.OnResourceAmountChanged += ResourceManager_OnResourceAmountChanged;
        CombatManager.OnBattleConcluded += CombatManager_OnBattleConcluded;
    }

    private void OnDestroy()
    {
        ResourceManager.OnResourceAmountChanged -= ResourceManager_OnResourceAmountChanged;
        CombatManager.OnBattleConcluded -= CombatManager_OnBattleConcluded;
    }

    private void ResourceManager_OnResourceAmountChanged(float _amount)
    {
        if (resourceCount == null) return;

        resourceCount.text = ((int)_amount).ToString();
        EvaluateButtons();
    }

    private void CombatManager_OnBattleConcluded(BattleResult _result)
    {
        CreateBattleResults(CombatManager.Instance.AdmiralInCombat, _result);
    }

    public override void Generate()
    {
        VisualElement container = new();
        container.AddToClassList("post-combat-container");
        Root.Add(container);

        background = new();
        background.AddToClassList("post-combat-background");
        container.Add(background);
    }

    private void EvaluateButtons()
    {
        Assert.IsTrue(postCombatButtons.All((b) => b != null));
        postCombatButtons.ForEach((b) => b.Evaluate());
    }

    public void CreateBattleResults(EnemyAdmiralController _enemyAdmiralController, BattleResult _result)
    {
        postCombatButtons.Clear();
        scrapActions.Clear();
        background.Clear();
        opacityElements.Clear();

        if (_result != BattleResult.Defeat)
        {
            CreateHeader(background, _result);
            CreateResoureContainer(background);

            Box resultsBackground = CreateBattleResultsContainer(background);
            CreatePlayerColumn(resultsBackground, true);
            CreateEnemyColumn(resultsBackground, _enemyAdmiralController, true);

            navigationButtonContainer = CreateNavigationButtonContainer(background);
            CreateContinueButton(navigationButtonContainer, _enemyAdmiralController);
            CreateScrapAllButton(navigationButtonContainer, _enemyAdmiralController);

            StartCoroutine(ShowPostCombatScreen());
        }

        else
        {
            CreateHeader(background, _result);

            Box resultsBackground = CreateBattleResultsContainer(background);
            CreatePlayerColumn(resultsBackground, false);
            CreateEnemyColumn(resultsBackground, _enemyAdmiralController, false);

            navigationButtonContainer = CreateNavigationButtonContainer(background);
            CreateDefeatButton(navigationButtonContainer);

            StartCoroutine(ShowPostCombatScreen());
        }

        ResourceManager_OnResourceAmountChanged(ResourceManager.Instance.Amount);
    }

    private void CreateHeader(VisualElement _parent, BattleResult _battleResult)
    {
        headerContainer = new();
        headerContainer.AddToClassList("post-combat-header-container");
        _parent.Add(headerContainer);

        Label header = new(_battleResult.ToString());
        header.AddToClassList("post-combat-header");
        SetFontSize(header, 70);
        SetHeight(header, HEADER_HEIGHT);
        headerContainer.Add(header);
    }

    // Resource

    private void CreateResoureContainer(VisualElement _parent)
    {
        resourceContainer = new();
        resourceContainer.AddToClassList("post-combat-resource-container");
        _parent.Add(resourceContainer);

        VisualElement resourceBackground = new();
        resourceBackground.AddToClassList("post-combat-resource-background");
        SetHeight(resourceContainer, RESOURCE_HEIGHT);
        resourceContainer.Add(resourceBackground);

        Label label = new("Resources: ");
        label.AddToClassList("post-combat-resource-label");
        SetFontSize(label, 30);
        resourceBackground.Add(label);

        resourceCount = new();
        resourceCount.AddToClassList("post-combat-resource-label");
        SetFontSize(resourceCount, 30);
        resourceBackground.Add(resourceCount);
    }

    private Box CreateBattleResultsContainer(VisualElement _parent)
    {
        battleResultsContainer = new();
        battleResultsContainer.AddToClassList("post-combat-column-container");
        _parent.Add(battleResultsContainer);

        battleResultsBackground = new();
        battleResultsBackground.AddToClassList("post-combat-column-background");
        SetHeight(battleResultsBackground, RESULTS_HEIGHT - 25);
        battleResultsContainer.Add(battleResultsBackground);

        return battleResultsBackground;
    }

    // Player

    private void CreatePlayerColumn(VisualElement _parent, bool _shoulCreateButtons)
    {
        VisualElement columnContainer = CreateColumnContainer(_parent);
        CreateColumnHeader(columnContainer, $"{PlayerBoatController.Instance.AdmiralController.Name}'s Fleet");
        ScrollView rowContainer = CreateRowScrollView(columnContainer);

        CreatePlayerRow(rowContainer, PlayerBoatController.Instance.Boat, _shoulCreateButtons);

        foreach (AIBoatController boatController in PlayerBoatController.Instance.AdmiralController.Subordinates)
        {
            CreatePlayerRow(rowContainer, boatController.Boat, _shoulCreateButtons);
        }
    }

    private void CreatePlayerRow(VisualElement _parent, Boat _boat, bool _shoulCreateButtons)
    {
        VisualElement container = CreateRow(_parent);
        Label description = CreateRowDescription(container, _boat);
        VisualElement buttonContainer = CreateButtonContainer(container);

        if (_shoulCreateButtons)
        {
            Button button = CreateRowButton(buttonContainer, "Repair", -ResourceManager.GetRepairCost(_boat), () => CanRepair(_boat));
            button.clicked += () => OnRepaired(description, _boat);
            button.clicked += () => OnBoatRepaired?.Invoke();
        }
    }

    private bool CanRepair(Boat _boat)
    {
        return _boat.IsDamaged && ResourceManager.Instance.CanRepair(_boat);
    }

    private void OnRepaired(Label _description, Boat _boat)
    {
        int cost = ResourceManager.GetRepairCost(_boat);
        _boat.Repair();
        _description.text = $"{_boat.Name}: Repaired";
        ResourceManager.Instance.BoatWasRepaired(cost);
    }

    // Enemy

    private void CreateEnemyColumn(VisualElement _parent, EnemyAdmiralController _admiral, bool _shoulCreateButtons)
    {
        VisualElement columnContainer = CreateColumnContainer(_parent);
        CreateColumnHeader(columnContainer, $"{_admiral.Name}'s Fleet");
        ScrollView rowContainer = CreateRowScrollView(columnContainer);
        CreateEnemyRow(rowContainer, _admiral.AIBoatController, _shoulCreateButtons);

        foreach (AIBoatController boatController in _admiral.Subordinates)
        {
            CreateEnemyRow(rowContainer, boatController, _shoulCreateButtons);
        }
    }

    private void CreateEnemyRow(VisualElement _parent, AIBoatController _boatController, bool _shoulCreateButtons)
    {
        VisualElement container = CreateRow(_parent);
        Label description = CreateRowDescription(container, _boatController.Boat);
        VisualElement buttonContainer = CreateButtonContainer(container);

        if (_shoulCreateButtons)
        {
            Button seizeButton = CreateRowButton(buttonContainer, "Seize", -ResourceManager.GetRepairCost(_boatController.Boat), () => CanSeize(_boatController));
            Button scrapButton = CreateRowButton(buttonContainer, "Scrap", ResourceManager.GAIN_FROM_SCRAPPING_AMOUNT, () => CanScrap(_boatController));

            seizeButton.clicked += () => OnSeized(description, _boatController);
            seizeButton.clicked += () => OnBoatSeized?.Invoke();
            scrapButton.clicked += () => OnScrapped(description, _boatController);
            scrapButton.clicked += () => OnBoatScrapped?.Invoke();

            scrapActions.Add(() => OnScrapped(description, _boatController));
        }
    }

    private bool CanScrap(AIBoatController _boatController)
    {
        return _boatController.Boat.IsSunk && _boatController.State == AIBoatControllerState.Active;
    }

    private bool CanSeize(AIBoatController _boatController)
    {
        return _boatController.Boat.IsSunk && _boatController.State == AIBoatControllerState.Active && ResourceManager.Instance.CanSeize(_boatController.Boat);
    }

    private void OnSeized(Label _description, AIBoatController _boatController)
    {
        int cost = ResourceManager.GetRepairCost(_boatController.Boat);
        _boatController.Seize(PlayerBoatController.Instance.AdmiralController);
        _description.text = $"{_boatController.Boat.Name}: Seized";
        ResourceManager.Instance.BoatWasSeized(cost);
    }

    private void OnScrapped(Label _description, AIBoatController _boatController)
    {
        if (CanScrap(_boatController))
        {
            _boatController.Scrap();
            _description.text = $"{_boatController.Boat.Name}: Scrapped";
            ResourceManager.Instance.BoatWasScrapped();
        }
    }

    // Components

    private VisualElement CreateColumnContainer(VisualElement _parent)
    {
        VisualElement container = new();
        container.AddToClassList("post-combat-column");
        _parent.Add(container);

        return container;
    }

    private void CreateColumnHeader(VisualElement _parent, string _name)
    {
        Label header = new(_name);
        header.AddToClassList("post-combat-column-header");
        SetFontSize(header, 33);
        _parent.Add(header);
    }

    private ScrollView CreateRowScrollView(VisualElement _parent)
    {
        ScrollView container = new();
        container.AddToClassList("post-combat-row-container");
        container.verticalScroller.highButton.RemoveFromHierarchy();
        container.verticalScroller.lowButton.RemoveFromHierarchy();
        container.horizontalScroller.RemoveFromHierarchy();
        _parent.Add(container);

        opacityElements.Add(container.verticalScroller);

        return container;
    }

    private VisualElement CreateRow(VisualElement _parent)
    {
        VisualElement container = new();
        container.AddToClassList("post-combat-row");
        _parent.Add(container);

        return container;
    }

    private Label CreateRowDescription(VisualElement _parent, Boat _boat)
    {
        Label description = new($"{_boat.Name}: {(_boat.IsSunk ? "Sunk" : $"Durability {_boat.GetPercentageHealth()}%")}");
        description.AddToClassList("post-combat-row-desciption");
        SetFontSize(description, 19);
        _parent.Add(description);

        return description;
    }

    private VisualElement CreateButtonContainer(VisualElement container)
    {
        VisualElement buttonContainer = new();
        buttonContainer.AddToClassList("post-combat-row-button-container");
        container.Add(buttonContainer);
        return buttonContainer;
    }

    private Button CreateRowButton(VisualElement _parent, string _text, int _resource, Func<bool> _isEnabled)
    {
        Button button = new();
        button.AddToClassList("post-combat-button");
        button.AddToClassList("post-combat-row-button");
        SetFontSize(button, 17);
        button.text = $"{_text} ({(_resource > 0 ? "+" : "-")} {Mathf.Abs(_resource)} R)";
        postCombatButtons.Add(new(button, _isEnabled));
        _parent.Add(button);

        return button;
    }

    // Navigation

    private VisualElement CreateNavigationButtonContainer(VisualElement _parent)
    {
        navigationButtonContainer = new();
        navigationButtonContainer.AddToClassList("post-combat-navigation-button-container");
        _parent.Add(navigationButtonContainer);

        VisualElement navigationBackground = new();
        navigationBackground.AddToClassList("post-combat-navigation-button-background");
        SetHeight(navigationBackground, NAVIGATION_HEIGHT);
        navigationButtonContainer.Add(navigationBackground);

        return navigationBackground;
    }

    private void CreateContinueButton(VisualElement _parent, EnemyAdmiralController _admiral)
    {
        Button button = new(() => CombatManager.Instance.BattleResultsCompleted());
        button.AddToClassList("post-combat-button");
        button.AddToClassList("post-combat-navigation-button");
        SetFontSize(button, 35);
        button.text = "Continue";
        postCombatButtons.Add(new(button, () => CanContinue(_admiral)));
        _parent.Add(button);
    }

    private bool CanContinue(EnemyAdmiralController _admiral)
    {
        return (_admiral.AIBoatController == null || !CanScrap(_admiral.AIBoatController)) && _admiral.Subordinates.All((s) => !CanScrap(s));
    }

    private void CreateScrapAllButton(VisualElement _parent, EnemyAdmiralController _admiral)
    {
        Button button = new(OnScrapAll);
        button.AddToClassList("post-combat-button");
        button.AddToClassList("post-combat-navigation-button");
        SetFontSize(button, 35);
        postCombatButtons.Add(new(button, () => CanScrapAll(_admiral)));
        button.text = "Scrap all";

        _parent.Add(button);
    }

    private bool CanScrapAll(EnemyAdmiralController _admiral)
    {
        return (_admiral.AIBoatController != null && CanScrap(_admiral.AIBoatController)) || _admiral.Subordinates.Any((s) => CanScrap(s));
    }

    private void OnScrapAll()
    {
        foreach (Action action in scrapActions)
        {
            action();
        }

        OnBoatScrapped?.Invoke();
    }

    private void CreateDefeatButton(VisualElement _parent)
    {
        Button button = new(() => SceneManager.LoadScene("Game"));
        button.AddToClassList("post-combat-button");
        button.AddToClassList("post-combat-navigation-button");
        SetFontSize(button, 35);
        button.text = "Return to main menu";
        _parent.Add(button);
    }

    public IEnumerator ShowPostCombatScreen()
    {
        const float BACKGROUND_BORDER_WIDTH = 6f;
        const float BACKGROUND_BORDER_DURATION = 0.1f;
        const float HEADER_HEIGHT_DURATION = 0.2f;

        const float BACKGROUND_WIDTH_DURATION = 0.6f;

        const float RESULTS_BORDER_WIDTH = 4f;
        const float RESULTS_BORDER_DURATION = 0.1f;

        const float HEIGHT_DURATION = 2f;

        const float OPACITY_DURATION = 0.4f;

        SetWidth(background, 0);
        SetBorder(background, 0);
        SetHeight(headerContainer, 0);

        SetBorder(battleResultsBackground, 0);

        SetHeight(resourceContainer, 0);
        SetHeight(battleResultsContainer, 0);
        SetHeight(navigationButtonContainer, 0);

        foreach (VisualElement opacityElement in opacityElements)
        {
            opacityElement.style.opacity = 0;
            opacityElement.enabledSelf = false;
        }

        yield return AnimateBorder(background, BACKGROUND_BORDER_DURATION, 0, BACKGROUND_BORDER_WIDTH);
        yield return AnimateHeight(headerContainer, HEADER_HEIGHT_DURATION, 0, HEADER_HEIGHT);

        yield return new WaitForSeconds(0.2f);

        yield return AnimateWidth(background, BACKGROUND_WIDTH_DURATION, 0, BACKGROUND_WIDTH);

        yield return new WaitForSeconds(2f);

        yield return AnimateBorder(battleResultsBackground, RESULTS_BORDER_DURATION, 0, RESULTS_BORDER_WIDTH);

        const float TOTAL_HEIGHT = RESOURCE_HEIGHT + RESULTS_HEIGHT + NAVIGATION_HEIGHT;

        yield return AnimateHeight(resourceContainer, RESOURCE_HEIGHT / TOTAL_HEIGHT * HEIGHT_DURATION, 0, RESOURCE_HEIGHT);
        yield return AnimateHeight(battleResultsContainer, RESULTS_HEIGHT / TOTAL_HEIGHT * HEIGHT_DURATION, 0, RESULTS_HEIGHT);
        yield return AnimateHeight(navigationButtonContainer, NAVIGATION_HEIGHT / TOTAL_HEIGHT * HEIGHT_DURATION, 0, NAVIGATION_HEIGHT);

        yield return new WaitForSeconds(0.5f);

        foreach (VisualElement opacityElement in opacityElements)
        {
            opacityElement.enabledSelf = true;
        }

        yield return AnimateOpacity(opacityElements, OPACITY_DURATION, 0, 1);
    }

    private class PostCombatButton
    {
        private readonly Button Button;
        private readonly Func<bool> IsActive;

        public PostCombatButton(Button _button, Func<bool> _activeFunc)
        {
            Button = _button;
            IsActive = _activeFunc;
        }

        public void Evaluate()
        {
            Button.SetEnabled(IsActive());
        }
    }
}