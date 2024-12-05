using System;
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

    private Box background;
    private Label resourceCount;

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
        root.Add(container);

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

        CreateHeader(background, _result);

        if (_result != BattleResult.Defeat)
        {
            Box battleResultsContainer = CreateBattleResultsContainer();

            CreatePlayerColumn(battleResultsContainer, true);
            CreateEnemyColumn(battleResultsContainer, _enemyAdmiralController, true);

            VisualElement navigationButtonContainer = CreateNavigationButtonContainer();
            CreateContinueButton(navigationButtonContainer, _enemyAdmiralController);
            CreateScrapAllButton(navigationButtonContainer, _enemyAdmiralController);

            CreateResoureContainer(background);
        }

        else
        {
            Box battleResultsContainer = CreateBattleResultsContainer();

            CreatePlayerColumn(battleResultsContainer, false);
            CreateEnemyColumn(battleResultsContainer, _enemyAdmiralController, false);

            VisualElement navigationButtonContainer = CreateNavigationButtonContainer();
            CreateDefeatButton(navigationButtonContainer);
        }

        ResourceManager_OnResourceAmountChanged(ResourceManager.Instance.Amount);
    }

    private void CreateHeader(VisualElement _container, BattleResult _battleResult)
    {
        Label header = new(_battleResult.ToString());
        header.AddToClassList("post-combat-header");
        SetFontSize(header, 70);
        _container.Add(header);
    }

    private Box CreateBattleResultsContainer()
    {
        Box battleResultsContainer = new();
        battleResultsContainer.AddToClassList("post-combat-column-container");
        background.Add(battleResultsContainer);
        return battleResultsContainer;
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
            Button button = CreateRowButton(buttonContainer, "Repair", ResourceManager.GetRepairCost(_boat), () => CanRepair(_boat));
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
            Button seizeButton = CreateRowButton(buttonContainer, "Seize", ResourceManager.GetRepairCost(_boatController.Boat), () => CanSeize(_boatController));
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
        _parent.Add(container);

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
        button.text = $"{_text} for {_resource} resources";
        postCombatButtons.Add(new(button, _isEnabled));
        _parent.Add(button);

        return button;
    }

    // Resource

    private void CreateResoureContainer(VisualElement _parent)
    {
        VisualElement resourceContainer = new();
        resourceContainer.AddToClassList("post-combat-resource-container");
        _parent.Add(resourceContainer);

        Label label = new("Resources: ");
        label.AddToClassList("post-combat-resource-label");
        SetFontSize(label, 30);
        resourceContainer.Add(label);

        resourceCount = new();
        resourceCount.AddToClassList("post-combat-resource-label");
        SetFontSize(resourceCount, 30);
        resourceContainer.Add(resourceCount);
    }

    // Navigation

    private VisualElement CreateNavigationButtonContainer()
    {
        VisualElement navigationButtonContainer = new();
        navigationButtonContainer.AddToClassList("post-combat-navigation-button-container");
        background.Add(navigationButtonContainer);

        return navigationButtonContainer;
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
        Button button = new(() => OnScrapAll(_admiral));
        button.AddToClassList("post-combat-button");
        button.AddToClassList("post-combat-navigation-button");
        SetFontSize(button, 35);
        postCombatButtons.Add(new(button, () => CanScrapAll(_admiral)));
        button.text = "Scrap all enemy boats";

        _parent.Add(button);
    }

    private bool CanScrapAll(EnemyAdmiralController _admiral)
    {
        return (_admiral.AIBoatController != null && CanScrap(_admiral.AIBoatController)) || _admiral.Subordinates.Any((s) => CanScrap(s));
    }

    private void OnScrapAll(EnemyAdmiralController _admiral)
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