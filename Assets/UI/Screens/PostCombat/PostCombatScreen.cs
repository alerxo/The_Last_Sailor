using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PostCombatScreen : UIScreen
{
    public static PostCombatScreen Instance { get; private set; }

    protected override UIState ActiveState => UIState.PostCombat;

    private Box background;

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;
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

    public void CreateBattleResults(EnemyAdmiralController _enemyAdmiralController)
    {
        background.Clear();

        BattleResult battleResult = GetBattleResult();

        CreateHeader(background, battleResult);

        if (battleResult != BattleResult.Defeat)
        {
            Box battleResultsContainer = new();
            battleResultsContainer.AddToClassList("post-combat-column-container");
            background.Add(battleResultsContainer);

            CreatePlayerColumn(battleResultsContainer);
            CreateEnemyColumn(battleResultsContainer, _enemyAdmiralController);

            CreateContinueButton(background);
        }

        else
        {
            CreateDefeatButton(background);
        }
    }

    public BattleResult GetBattleResult()
    {
        if (PlayerBoatController.Instance.AdmiralController.Fleet.All((b) => b.IsSunk))
        {
            return BattleResult.Defeat;
        }

        return BattleResult.Victory;
    }

    private void CreateHeader(VisualElement _container, BattleResult _battleResult)
    {
        Label header = new(_battleResult.ToString());
        header.AddToClassList("post-combat-header");
        SetFontSize(header, 70);
        _container.Add(header);
    }

    // Player

    private void CreatePlayerColumn(VisualElement _parent)
    {
        VisualElement columnContainer = CreateColumnContainer(_parent);
        CreateColumnHeader(columnContainer, "Player Fleet");
        ScrollView rowContainer = CreateRowScrollView(columnContainer);

        CreatePlayerRow(rowContainer, PlayerBoatController.Instance.Boat, "Player Boat");

        foreach (AIBoatController boatController in PlayerBoatController.Instance.AdmiralController.Subordinates)
        {
            CreatePlayerRow(rowContainer, boatController.Boat, "Allied Boat");
        }
    }

    private void CreatePlayerRow(VisualElement _parent, Boat _boat, string _boatName)
    {
        VisualElement container = CreateRow(_parent);
        Label description = CreateRowDescription(container, _boat, _boatName);
        Button button = CreateRowButton(container, "Repair", _boat.IsDamaged);
        button.clicked += () => OnRepaired(button, description, _boatName, _boat);
    }

    private void OnRepaired(Button _button, Label _description, string _boatName, Boat _boat)
    {
        _boat.Repair();
        _description.text = $"{_boatName}: Repaired";
        _button.SetEnabled(false);
    }

    // Enemy

    private void CreateEnemyColumn(VisualElement _parent, EnemyAdmiralController _admiral)
    {
        VisualElement columnContainer = CreateColumnContainer(_parent);
        CreateColumnHeader(columnContainer, $"{_admiral.Name}'s Fleet");
        ScrollView rowContainer = CreateRowScrollView(columnContainer);
        CreateEnemyRow(rowContainer, _admiral.BoatController, $"{_admiral.Name}");

        foreach (AIBoatController boatController in _admiral.Subordinates)
        {
            CreateEnemyRow(rowContainer, boatController, $"Enemy Boat");
        }
    }

    private void CreateEnemyRow(VisualElement _parent, AIBoatController _boatController, string _boatName)
    {
        VisualElement container = CreateRow(_parent);
        Label description = CreateRowDescription(container, _boatController.Boat, _boatName);
        Button button = CreateRowButton(container, "Seize", _boatController.Boat.IsSunk);
        button.clicked += () => OnSeized(button, description, _boatName, _boatController);
    }

    private void OnSeized(Button _button, Label _description, string _boatName, AIBoatController _boatController)
    {
        _boatController.Seize(PlayerBoatController.Instance.AdmiralController);
        _description.text = $"{_boatName}: Seized";
        _button.SetEnabled(false);
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

    private Label CreateRowDescription(VisualElement _parent, Boat _boat, string _name)
    {
        Label description = new($"{_name}: {(_boat.IsSunk ? "Sunk" : $"{_boat.Health} Health")}");
        description.AddToClassList("post-combat-row-desciption");
        SetFontSize(description, 20);
        _parent.Add(description);

        return description;
    }

    private Button CreateRowButton(VisualElement _parent, string _text, bool _isEnabled)
    {
        Button button = new();
        button.AddToClassList("post-combat-button");
        button.AddToClassList("post-combat-row-button");
        SetFontSize(button, 18);
        button.text = _text;
        button.SetEnabled(_isEnabled);
        _parent.Add(button);

        return button;
    }

    // Navigation

    private void CreateContinueButton(VisualElement _parent)
    {
        Button button = new(() => CombatManager.Instance.BattleResultsCompleted());
        button.AddToClassList("post-combat-button");
        button.AddToClassList("post-combat-navigation-button");
        SetFontSize(button, 35);
        button.text = "Continue";
        _parent.Add(button);
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
}

public enum BattleResult
{
    Defeat,
    Victory
}