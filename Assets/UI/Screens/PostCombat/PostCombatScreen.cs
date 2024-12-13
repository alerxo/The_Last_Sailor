using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PostCombatScreen : UIScreen
{
    public static PostCombatScreen Instance { get; private set; }

    protected override List<UIState> ActiveStates => new() { UIState.PostCombat };

    private Box background;
    private VisualElement content;
    private Label resourceCount;
    private readonly List<ScrollView> resultColumns = new();
    private readonly List<VisualElement> playerColumnItems = new();
    private readonly List<VisualElement> enemyColumnItems = new();

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
        CreateBattleResults(CombatManager.Instance.Enemy, _result);
    }

    public override void Generate()
    {
        VisualElement container = new();
        container.AddToClassList("post-combat-container");
        Root.Add(container);

        background = new();
        background.AddToClassList("post-combat-background");
        SetMargin(background, 100, 0, 0, 0);
        SetBorderRadius(background, 10);
        container.Add(background);
    }

    public void CreateBattleResults(EnemyAdmiralController _enemyAdmiralController, BattleResult _result)
    {
        background.Clear();
        resultColumns.Clear();
        playerColumnItems.Clear();
        enemyColumnItems.Clear();

        CreateHeader(background, _result);
        VisualElement content = CreateContentContainer();

        if (_result != BattleResult.Defeat) CreateResoureContainer(content);

        Box resultsContainer = CreateBattleResultsBackground(content);
        CreatePlayerColumn(resultsContainer);
        CreateEnemyColumn(resultsContainer, _enemyAdmiralController);

        if (_result != BattleResult.Defeat) CreateContinueButton(content);
        else CreateDefeatButton(content);

        StartCoroutine(ShowPostCombatScreen());
        ResourceManager_OnResourceAmountChanged(ResourceManager.Instance.Amount);
        UIManager.DisableTab(background);
    }

    private void CreateHeader(VisualElement _parent, BattleResult _battleResult)
    {
        Label header = new(_battleResult.ToString());
        header.AddToClassList("post-combat-header");
        SetMargin(header, 20);
        SetFontSize(header, 70);
        _parent.Add(header);
    }

    private VisualElement CreateContentContainer()
    {
        content = new();
        content.AddToClassList("post-combat-content");
        SetMargin(content, 0, 0, 75, 75);
        background.Add(content);

        return content;
    }

    // Resource

    private void CreateResoureContainer(VisualElement _parent)
    {
        VisualElement resourceContainer = new();
        resourceContainer.AddToClassList("post-combat-resource-container");
        SetMargin(resourceContainer, 0, 30, 0, 0);
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

    private Box CreateBattleResultsBackground(VisualElement _parent)
    {
        Box battleResultsBackground = new();
        battleResultsBackground.AddToClassList("post-combat-column-background");
        SetBorderWidthRadius(battleResultsBackground, 4, 7);
        _parent.Add(battleResultsBackground);

        return battleResultsBackground;
    }

    // Player

    private void CreatePlayerColumn(VisualElement _parent)
    {
        VisualElement columnContainer = CreateColumnContainer(_parent);
        CreateColumnHeader(columnContainer, $"{PlayerBoatController.Instance.AdmiralController.Name}'s Fleet");
        ScrollView rowContainer = CreateRowScrollView(columnContainer);

        CreateRow(rowContainer, PlayerBoatController.Instance.Boat, true);

        foreach (AIBoatController boatController in PlayerBoatController.Instance.AdmiralController.Subordinates)
        {
            CreateRow(rowContainer, boatController.Boat, true);
        }
    }

    // Enemy

    private void CreateEnemyColumn(VisualElement _parent, EnemyAdmiralController _admiral)
    {
        VisualElement columnContainer = CreateColumnContainer(_parent);
        CreateColumnHeader(columnContainer, $"{_admiral.Name}'s Fleet");
        ScrollView rowContainer = CreateRowScrollView(columnContainer);

        CreateRow(rowContainer, _admiral.AIBoatController.Boat, false);

        foreach (AIBoatController boatController in _admiral.Subordinates)
        {
            CreateRow(rowContainer, boatController.Boat, false);
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
        ScrollView scrollView = new();
        scrollView.AddToClassList("post-combat-row-container");
        scrollView.verticalScroller.highButton.RemoveFromHierarchy();
        scrollView.verticalScroller.lowButton.RemoveFromHierarchy();
        scrollView.horizontalScroller.RemoveFromHierarchy();
        _parent.Add(scrollView);

        resultColumns.Add(scrollView);

        return scrollView;
    }

    private void CreateRow(VisualElement _parent, Boat _boat, bool _isPlayer)
    {
        VisualElement container = new();
        container.AddToClassList("post-combat-row");
        SetMargin(container, 10, 0, 0, 0);
        SetPadding(container, 0, 4, 0, 0);
        container.style.borderBottomWidth = GetScaledValue(2);
        _parent.Add(container);

        Label description = new($"{_boat.Name}: {(_boat.IsSunk ? "Sunk" : $"Durability {_boat.GetPercentageDurability()}%")}");
        description.AddToClassList("post-combat-row-desciption");
        SetFontSize(description, 19);
        container.Add(description);

        (_isPlayer ? playerColumnItems : enemyColumnItems).Add(container);
    }

    // Navigation

    private void CreateContinueButton(VisualElement _parent)
    {
        Button button = new(() => CombatManager.Instance.BattleResultsCompleted());
        button.AddToClassList("main-button");
        button.AddToClassList("post-combat-navigation-button");
        SetMargin(button, 40, 40, 0, 0);
        SetPadding(button, 20, 20, 50, 50);
        SetBorderWidthRadius(button, 5, 16);
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
        HideAnimatedItems();

        yield return new WaitForSeconds(0.25f);

        yield return AnimateBorderWidth(background, 0.1f, 0, 5);
        yield return AnimateWidth(background, 1f, 0, 1000);

        yield return new WaitForSeconds(1f);

        yield return AnimateHeight(content, 0.7f, 0, 700);

        yield return new WaitForSeconds(0.1f);

        for (int i = 0; i < Mathf.Max(playerColumnItems.Count, enemyColumnItems.Count); i++)
        {
            if (playerColumnItems.Count > i)
            {
                playerColumnItems[i].SetEnabled(true);
            }

            if (enemyColumnItems.Count > i)
            {
                enemyColumnItems[i].SetEnabled(true);
            }

            yield return new WaitForSeconds(0.1f);
        }

        foreach (ScrollView scrollView in resultColumns)
        {
            scrollView.verticalScrollerVisibility = ScrollerVisibility.Auto;
        }
    }

    private void HideAnimatedItems()
    {
        SetBorderWidth(background, 0);
        SetWidth(background, 0);
        SetHeight(content, 0);

        for (int i = 0; i < Mathf.Max(playerColumnItems.Count, enemyColumnItems.Count); i++)
        {
            if (playerColumnItems.Count > i)
            {
                playerColumnItems[i].SetEnabled(false);
            }

            if (enemyColumnItems.Count > i)
            {
                enemyColumnItems[i].SetEnabled(false);
            }
        }

        foreach (ScrollView scrollView in resultColumns)
        {
            scrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
        }
    }
}