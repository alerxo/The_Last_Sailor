using System;
using System.Collections;
using System.Collections.Generic;
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
    private readonly List<VisualElement> battleResultItemsPlayer = new();
    private readonly List<VisualElement> battleResultItemsEnemy = new();
    private readonly List<VisualElement> scrollers = new();

    private const float NAVIGATION_HEIGHT = 140f;
    private VisualElement navigationButtonContainer;

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

    public void CreateBattleResults(EnemyAdmiralController _enemyAdmiralController, BattleResult _result)
    {
        background.Clear();
        battleResultItemsPlayer.Clear();
        battleResultItemsEnemy.Clear();
        scrollers.Clear();

        if (_result != BattleResult.Defeat)
        {
            CreateHeader(background, _result);
            CreateResoureContainer(background);

            Box resultsBackground = CreateBattleResultsContainer(background);
            CreatePlayerColumn(resultsBackground);
            CreateEnemyColumn(resultsBackground, _enemyAdmiralController);

            navigationButtonContainer = CreateNavigationButtonContainer(background);
            CreateContinueButton(navigationButtonContainer, _enemyAdmiralController);

            StartCoroutine(ShowPostCombatScreen());
        }

        else
        {
            CreateHeader(background, _result);

            Box resultsBackground = CreateBattleResultsContainer(background);
            CreatePlayerColumn(resultsBackground);
            CreateEnemyColumn(resultsBackground, _enemyAdmiralController);

            navigationButtonContainer = CreateNavigationButtonContainer(background);
            CreateDefeatButton(navigationButtonContainer);

            StartCoroutine(ShowPostCombatScreen());
        }

        ResourceManager_OnResourceAmountChanged(ResourceManager.Instance.Amount);

        UIManager.DisableTab(background);
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

    private void CreatePlayerColumn(VisualElement _parent)
    {
        VisualElement columnContainer = CreateColumnContainer(_parent);
        CreateColumnHeader(columnContainer, $"{PlayerBoatController.Instance.AdmiralController.Name}'s Fleet");
        ScrollView rowContainer = CreateRowScrollView(columnContainer);

        CreatePlayerRow(rowContainer, PlayerBoatController.Instance.Boat);

        foreach (AIBoatController boatController in PlayerBoatController.Instance.AdmiralController.Subordinates)
        {
            CreatePlayerRow(rowContainer, boatController.Boat);
        }
    }

    private void CreatePlayerRow(VisualElement _parent, Boat _boat)
    {
        VisualElement container = CreateRow(_parent);
        CreateRowDescription(container, _boat);

        container.SetEnabled(false);
        battleResultItemsPlayer.Add(container);
    }

    // Enemy

    private void CreateEnemyColumn(VisualElement _parent, EnemyAdmiralController _admiral)
    {
        VisualElement columnContainer = CreateColumnContainer(_parent);
        CreateColumnHeader(columnContainer, $"{_admiral.Name}'s Fleet");
        ScrollView rowContainer = CreateRowScrollView(columnContainer);

        CreateEnemyRow(rowContainer, _admiral.AIBoatController);

        foreach (AIBoatController boatController in _admiral.Subordinates)
        {
            CreateEnemyRow(rowContainer, boatController);
        }
    }

    private void CreateEnemyRow(VisualElement _parent, AIBoatController _boatController)
    {
        VisualElement container = CreateRow(_parent);
        CreateRowDescription(container, _boatController.Boat);

        container.SetEnabled(false);
        battleResultItemsEnemy.Add(container);
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
        ScrollView scrollView = new();
        scrollView.AddToClassList("post-combat-row-container");
        scrollView.verticalScroller.highButton.RemoveFromHierarchy();
        scrollView.verticalScroller.lowButton.RemoveFromHierarchy();
        scrollView.horizontalScroller.RemoveFromHierarchy();
        _parent.Add(scrollView);

        scrollers.Add(scrollView);

        return scrollView;
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
        button.AddToClassList("main-button");
        button.AddToClassList("post-combat-navigation-button");
        SetFontSize(button, 35);
        button.text = "Continue";
        _parent.Add(button);
    }

    private void CreateDefeatButton(VisualElement _parent)
    {
        Button button = new(() => SceneManager.LoadScene("Game"));
        button.AddToClassList("main-button");
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

        SetWidth(background, 0);
        SetBorder(background, 0);
        SetHeight(headerContainer, 0);

        SetBorder(battleResultsBackground, 0);

        SetHeight(resourceContainer, 0);
        SetHeight(battleResultsContainer, 0);
        SetHeight(navigationButtonContainer, 0);

        foreach (VisualElement scroller in scrollers)
        {
            scroller.SetEnabled(false);
        }

        yield return AnimateBorder(background, BACKGROUND_BORDER_DURATION, 0, BACKGROUND_BORDER_WIDTH);
        yield return AnimateHeight(headerContainer, HEADER_HEIGHT_DURATION, 0, HEADER_HEIGHT);

        yield return new WaitForSeconds(0.1f);

        yield return AnimateWidth(background, BACKGROUND_WIDTH_DURATION, 0, BACKGROUND_WIDTH);

        yield return new WaitForSeconds(1f);

        yield return AnimateBorder(battleResultsBackground, RESULTS_BORDER_DURATION, 0, RESULTS_BORDER_WIDTH);

        const float TOTAL_HEIGHT = RESOURCE_HEIGHT + RESULTS_HEIGHT + NAVIGATION_HEIGHT;

        yield return AnimateHeight(resourceContainer, RESOURCE_HEIGHT / TOTAL_HEIGHT * HEIGHT_DURATION, 0, RESOURCE_HEIGHT);
        yield return AnimateHeight(battleResultsContainer, RESULTS_HEIGHT / TOTAL_HEIGHT * HEIGHT_DURATION, 0, RESULTS_HEIGHT);
        yield return AnimateHeight(navigationButtonContainer, NAVIGATION_HEIGHT / TOTAL_HEIGHT * HEIGHT_DURATION, 0, NAVIGATION_HEIGHT);

        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < Mathf.Max(battleResultItemsPlayer.Count, battleResultItemsEnemy.Count); i++)
        {
            if (battleResultItemsPlayer.Count > i)
            {
                battleResultItemsPlayer[i].SetEnabled(true);
            }

            if (battleResultItemsEnemy.Count > i)
            {
                battleResultItemsEnemy[i].SetEnabled(true);
            }

            yield return new WaitForSeconds(0.2f);
        }

        foreach (VisualElement scroller in scrollers)
        {
            scroller.SetEnabled(true);
        }
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