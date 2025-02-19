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

    [SerializeField] private Texture2D boatIcon, sunkIcon, resourceIcon;

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

        CombatManager.OnBattleConcluded += CombatManager_OnBattleConcluded;
    }

    private void OnDestroy()
    {
        CombatManager.OnBattleConcluded -= CombatManager_OnBattleConcluded;
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

        switch (_result)
        {
            case BattleResult.Defeat:
                CreateDefeatButton(content);
                break;

            case BattleResult.Victory:
                CreateContinueButton(content);
                break;

            case BattleResult.BossDefeated:
                CreateBossDefeatedContainer(content);
                break;
        }

        float resourceStart = ResourceManager.Instance.Amount;
        float resourceGain = ResourceManager.Instance.GetEnemyFleetWorth();
        ResourceManager.Instance.AddResource(resourceGain);
        float resourceEnd = resourceStart + resourceGain;

        StartCoroutine(ShowPostCombatScreen(resourceStart, resourceEnd, _result == BattleResult.Defeat));
        UIManager.DisableTab(background);
    }

    private void CreateHeader(VisualElement _parent, BattleResult _battleResult)
    {
        Label header = new();
        header.AddToClassList("post-combat-header");
        SetMargin(header, 20);
        SetFontSize(header, 70);
        _parent.Add(header);

        switch (_battleResult)
        {
            case BattleResult.Defeat:
            case BattleResult.Victory:
                header.text = _battleResult.ToString();
                break;

            case BattleResult.BossDefeated:
                header.text = "Final Admiral Defeated";
                break;
        }
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

        Image image = new();
        SetSize(image, 64, 64);
        SetMargin(image, 0, 0, 0, 10);
        image.image = resourceIcon;
        resourceContainer.Add(image);

        resourceCount = new(ResourceManager.Instance.Amount.ToString());
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

        Image image = new();
        SetSize(image, 32, 32);
        SetMargin(image, 0, 0, 0, 10);
        image.image = _boat.IsSunk ? sunkIcon : boatIcon;
        container.Add(image);

        Label description = new($"{_boat.Name}: {(_boat.IsSunk ? "Sunk" : $"Durability {_boat.GetPercentageDurability()}%")}");
        description.AddToClassList("post-combat-row-desciption");
        SetFontSize(description, 19);
        container.Add(description);

        (_isPlayer ? playerColumnItems : enemyColumnItems).Add(container);
    }

    // Navigation

    private void CreateDefeatButton(VisualElement _parent)
    {
        Button button = new(() => SceneManager.LoadScene("Game"));
        button.AddToClassList("main-button");
        button.AddToClassList("post-combat-navigation-button");
        SetMargin(button, 40, 40, 0, 0);
        SetPadding(button, 20, 20, 50, 50);
        SetBorderWidthRadius(button, 5, 16);
        SetFontSize(button, 35);
        button.text = "Return to main menu";
        _parent.Add(button);
    }

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

    private void CreateBossDefeatedContainer(VisualElement _parent)
    {
        VisualElement container = new();
        container.AddToClassList("post-combat-navigation-button-container");
        _parent.Add(container);

        Button mainMenu = new(() => OnMainMenu());
        mainMenu.AddToClassList("main-button");
        mainMenu.AddToClassList("post-combat-navigation-button");
        SetMargin(mainMenu, 40, 40, 0, 40);
        SetPadding(mainMenu, 20, 20, 50, 50);
        SetBorderWidthRadius(mainMenu, 5, 16);
        SetFontSize(mainMenu, 35);
        mainMenu.text = "Main Menu";
        _parent.Add(mainMenu);

        Button freeplay = new(() => CombatManager.Instance.BattleResultsCompleted());
        freeplay.AddToClassList("main-button");
        freeplay.AddToClassList("post-combat-navigation-button");
        SetMargin(freeplay, 40, 40, 40, 0);
        SetPadding(freeplay, 20, 20, 50, 50);
        SetBorderWidthRadius(freeplay, 5, 16);
        SetFontSize(freeplay, 35);
        freeplay.text = "Endless Freeplay";
        _parent.Add(freeplay);
    }

    private void OnMainMenu()
    {
        UIManager.Instance.SetState(UIState.TitleScreen);
        SceneManager.LoadScene("Game");
    }

    public IEnumerator ShowPostCombatScreen(float _startResource, float _endResource, bool _isDeathScreen)
    {
        HideAnimatedItems(_isDeathScreen);

        yield return new WaitForSeconds(0.25f);

        yield return AnimateBorderWidth(background, 0.1f, 0, 5);
        yield return AnimateWidth(background, 1f, 0, 1000);
        yield return new WaitForSeconds(0.75f);

        yield return AnimateHeight(content, 0.7f, 0, 700);
        yield return new WaitForSeconds(0.25f);

        yield return AnimateScrollviewScrollDown(resultColumns, playerColumnItems, enemyColumnItems, 0.25f);
        yield return new WaitForSeconds(0.6f);

        yield return AnimateScrollviewScrollUp(resultColumns, 1f);
        yield return new WaitForSeconds(0.25f);

        yield return AnimateOpacity(resourceCount.parent, 0.5f, 0f, 1f);

        if (!_isDeathScreen)
        {
            yield return AnimateNumber(resourceCount, 1.5f, _startResource, _endResource);
            yield return new WaitForSeconds(0.25f);
        }

        foreach (ScrollView scrollView in resultColumns)
        {
            scrollView.verticalScrollerVisibility = ScrollerVisibility.Auto;
        }
    }

    private void HideAnimatedItems(bool _isDeathScreen)
    {
        SetBorderWidth(background, 0);
        SetWidth(background, 0);
        SetHeight(content, 0);

        if (!_isDeathScreen)
        {
            SetOpacity(resourceCount.parent, 0);
        }

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