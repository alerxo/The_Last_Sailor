using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class FleetScreen : UIScreen
{
    public static event UnityAction OnBoatUpgraded;
    protected override List<UIState> ActiveStates => new() { UIState.Fleet };

    private VisualElement container;
    private int currentIndex;


    private void Awake()
    {
        UIManager.OnStateChanged += UIManager_OnStateChanged;
    }

    private void OnDestroy()
    {
        UIManager.OnStateChanged -= UIManager_OnStateChanged;
    }

    private void UIManager_OnStateChanged(UIState _state)
    {
        if (container != null && _state == UIState.Fleet)
        {
            currentIndex = 0;
            Draw();
        }
    }

    public override void Generate()
    {
        container = new();
        container.AddToClassList("fleet-container");
        Root.Add(container);
    }

    private void Draw()
    {
        container.Clear();

        CreateBoatList(container);
        CreateCurrentContainer(container);
    }

    private void CreateBoatList(VisualElement _parent)
    {
        Box boatListBackground = new();
        boatListBackground.AddToClassList("fleet-boat-list-background");
        SetMargin(boatListBackground, 0, 100, 0, 20);
        SetPadding(boatListBackground, 10);
        SetBorderWidthRadius(boatListBackground, 0, 10);
        _parent.Add(boatListBackground);

        ScrollView boatListContainer = new();
        boatListContainer.AddToClassList("fleet-boat-list-container");
        boatListContainer.verticalScroller.highButton.RemoveFromHierarchy();
        boatListContainer.verticalScroller.lowButton.RemoveFromHierarchy();
        boatListContainer.horizontalScroller.RemoveFromHierarchy();
        boatListBackground.Add(boatListContainer);

        CreateBuildButton(boatListContainer);

        for (int i = 0; i < PlayerBoatController.Instance.AdmiralController.Fleet.Count; i++)
        {
            CreateBoatItem(boatListContainer, PlayerBoatController.Instance.AdmiralController.Fleet[i], i);
        }
    }

    private void CreateBuildButton(VisualElement _parent)
    {
        Button button = new(() => OnBuild());
        button.AddToClassList("main-button");
        button.AddToClassList("fleet-boat-list-item");
        SetMargin(button, 0, 10, 0, 0);
        SetBorderWidthRadius(button, 5, 10);
        SetFontSize(button, 25);
        button.text = $"Build New Boat (-{ResourceManager.Instance.GetBuildCost()} R)";
        button.SetEnabled(ResourceManager.Instance.CanBuild() && PlayerBoatController.Instance.AdmiralController.CanBuild);
        _parent.Add(button);
    }

    private void OnBuild()
    {
        ResourceManager.Instance.BuildPlayerBoat();
        currentIndex = PlayerBoatController.Instance.AdmiralController.Fleet.Count - 1;
        Draw();
    }

    private Button CreateBoatItem(VisualElement _parent, Boat _boat, int _index)
    {
        Button button = new(() => OnBoatItem(_index));
        button.AddToClassList("main-button");
        button.AddToClassList("fleet-boat-list-item");
        SetMargin(button, 0, 10, 0, 0);
        SetBorderWidthRadius(button, 5, 10);
        SetFontSize(button, 25);
        button.text = $"{_boat.Name} (Durability: {_boat.GetPercentageDurability()}%)";
        _parent.Add(button);

        return button;
    }

    private void OnBoatItem(int _index)
    {
        currentIndex = _index;
        Draw();
    }

    private void CreateCurrentContainer(VisualElement _parent)
    {
        VisualElement menuContainer = new();
        menuContainer.AddToClassList("fleet-current-container");
        SetMargin(menuContainer, 0, 20, 0, 0);
        _parent.Add(menuContainer);

        if (PlayerBoatController.Instance.AdmiralController.Fleet.Count > 1)
        {
            CreateNavigationButton(menuContainer, "<", -1);
        }

        CreateBoatContainer(menuContainer);

        if (PlayerBoatController.Instance.AdmiralController.Fleet.Count > 1)
        {
            CreateNavigationButton(menuContainer, ">", 1);
        }
    }

    private void CreateNavigationButton(VisualElement _parent, string _text, int _index)
    {
        Button button = new(() => OnNavigationArrow(_index));
        button.AddToClassList("main-button");
        button.AddToClassList("fleet-navigation-arrow-button");
        SetBorderWidthRadius(button, 3, 7);
        SetFontSize(button, 40);
        button.text = _text;
        _parent.Add(button);
    }

    public void OnNavigationArrow(int _index)
    {
        currentIndex += _index;

        if (currentIndex < 0) currentIndex = PlayerBoatController.Instance.AdmiralController.Fleet.Count - 1;
        else if (currentIndex > PlayerBoatController.Instance.AdmiralController.Fleet.Count - 1) currentIndex = 0;

        Draw();
    }

    private void CreateBoatContainer(VisualElement _parent)
    {
        Box container = new();
        container.AddToClassList("fleet-current-boat");
        SetMargin(container, 0, 0, 25, 25);
        SetPadding(container, 20);
        SetBorderWidthRadius(container, 0, 10);
        _parent.Add(container);

        Boat boat = PlayerBoatController.Instance.AdmiralController.Fleet[currentIndex];
        CameraManager.Instance.SetFleetCamera(boat.transform);

        CreateTopRow(container, boat);
        CreateUpgradeButtons(container, boat, currentIndex == 0);
    }

    private void CreateTopRow(VisualElement _parent, Boat _boat)
    {
        VisualElement container = new();
        container.AddToClassList("fleet-current-top-row");
        SetMargin(container, 0, 30, 0, 0);
        _parent.Add(container);

        Label resourceLabel = new($"Resources: {ResourceManager.Instance.Amount}");
        resourceLabel.AddToClassList("fleet-current-resource-label");
        SetFontSize(resourceLabel, 28);
        container.Add(resourceLabel);

        Label header = new($"{_boat.Name} (Durability: {_boat.GetPercentageDurability()})");
        header.AddToClassList("fleet-current-header");
        SetFontSize(header, 40);
        container.Add(header);

        VisualElement exitButtonContainer = new();
        exitButtonContainer.AddToClassList("fleet-current-exit-container");
        container.Add(exitButtonContainer);

        Button exitButton = new(() => OnExit());
        exitButton.AddToClassList("main-button");
        exitButton.AddToClassList("fleet-current-exit-button");
        SetBorderWidthRadius(exitButton, 3, 7);
        SetFontSize(exitButton, 32);
        exitButton.text = "X";
        exitButtonContainer.Add(exitButton);
    }

    private void OnExit()
    {
        UIManager.Instance.SetState(UIState.HUD);
        CameraManager.Instance.SetState(CameraState.Player);
        FirstPersonController.Instance.SetState(PlayerState.FirstPerson);
    }

    private void CreateUpgradeButtons(VisualElement _parent, Boat _boat, bool _isPlayer)
    {
        VisualElement buttonContainer = new();
        buttonContainer.AddToClassList("fleet-current-upgrade-container");
        _parent.Add(buttonContainer);

        CreateRepairButton(buttonContainer, _boat);

        if (_isPlayer)
        {
            CreateFleetCapUpgradeButton(buttonContainer, _boat.GetComponent<PlayerAdmiralController>());
        }

        CreateUpgradeButton(buttonContainer, _boat, UpgradeType.Hull).clicked += () => _boat.Repair();
        CreateUpgradeButton(buttonContainer, _boat, UpgradeType.Cannons);
    }

    private void CreateRepairButton(VisualElement _parent, Boat _boat)
    {
        VisualElement container = new();
        container.AddToClassList("fleet-current-upgrade");
        _parent.Add(container);

        Button button = new(() => OnRepair(_boat));
        button.AddToClassList("main-button");
        button.AddToClassList("fleet-current-upgrade-button");
        SetBorderWidthRadius(button, 3, 7);
        SetFontSize(button, 26);
        button.text = "Repair (-10 R)";
        button.SetEnabled(_boat.IsDamaged && ResourceManager.Instance.CanRepair(_boat));
        container.Add(button);
    }

    private void OnRepair(Boat _boat)
    {
        ResourceManager.Instance.ReparBoat(_boat);
        Draw();
    }

    private Button CreateFleetCapUpgradeButton(VisualElement _parent, PlayerAdmiralController _admiral)
    {
        VisualElement container = new();
        container.AddToClassList("fleet-current-upgrade");
        SetMargin(container, 0, 0, 10, 0);
        _parent.Add(container);

        Label description = new($"Tier {string.Concat(Enumerable.Repeat("I", _admiral.SuborinateUpgradeIndex))} Fleet Size ({_admiral.GetSubordinateCap})");
        description.AddToClassList("fleet-current-upgrade-description");
        SetFontSize(description, 22);
        container.Add(description);

        Button button = new(() => OnSubordinateCap(_admiral));
        button.AddToClassList("main-button");
        button.AddToClassList("fleet-current-upgrade-button");
        SetBorderWidthRadius(button, 3, 7);
        SetFontSize(button, 22);
        button.text = $"+ {_admiral.GetSubordinateCapIncrease} Fleet Size (-{_admiral.GetSubordinateUpgradeCost} R)";
        button.SetEnabled(ResourceManager.Instance.Amount >= _admiral.GetSubordinateUpgradeCost && _admiral.CanUpgradeSubodinateCap);
        container.Add(button);

        return button;
    }

    private void OnSubordinateCap(PlayerAdmiralController _admiral)
    {
        _admiral.UpgradeSuborniateCap();
        OnBoatUpgraded?.Invoke();
        Draw();
    }

    private Button CreateUpgradeButton(VisualElement _parent, Boat _boat, UpgradeType _type)
    {
        VisualElement container = new();
        container.AddToClassList("fleet-current-upgrade");
        SetMargin(container, 0, 0, 10, 0);
        _parent.Add(container);

        Label description = new($"Tier {_boat.GetTierOfUpgradeAsString(_type)} {_type} ({_boat.GetUpgradeModifierPercentage(_type)}% {_boat.GetModifierDescription(_type)})");
        description.AddToClassList("fleet-current-upgrade-description");
        SetFontSize(description, 22);
        container.Add(description);

        Button button = new(() => OnUpgrade(_boat, _type));
        button.AddToClassList("main-button");
        button.AddToClassList("fleet-current-upgrade-button");
        SetBorderWidthRadius(button, 3, 7);
        SetFontSize(button, 22);
        button.text = $"+ {_boat.GetUpgradeIncreasePercentage(_type)}% {_boat.GetModifierDescription(_type)} (-{Boat.UPGRADE_COST} R)";
        button.SetEnabled(_boat.CanUpgrade(_type));
        container.Add(button);

        return button;
    }

    private void OnUpgrade(Boat _boat, UpgradeType _type)
    {
        _boat.Upgrade(_type);
        OnBoatUpgraded?.Invoke();
        Draw();
    }
}
