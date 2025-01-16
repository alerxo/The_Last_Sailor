using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class FleetScreen : UIScreen
{
    public static event UnityAction OnBoatUpgraded;
    public static event UnityAction OnBoatBuilt;
    public static event UnityAction OnBoatRepaired;
    protected override List<UIState> ActiveStates => new() { UIState.Fleet };

    [SerializeField] private Texture2D resourceIcon, upgradeIcon, repairIcon, boatIcon, buildIcon;

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
            if (PlayerBoatController.Instance.AdmiralController.Enemy == null)
            {
                currentIndex = 0;
                Draw();
            }

            else
            {
                DrawPopup();
                CameraManager.Instance.SetFleetCamera(PlayerBoatController.Instance.Boat.transform);
            }
        }
    }

    public override void Generate()
    {
        container = new();
        container.AddToClassList("fleet-container");
        Root.Add(container);
    }

    private void DrawPopup()
    {
        container.Clear();

        VisualElement popupContainer = new();
        popupContainer.AddToClassList("fleet-popup-container");
        container.Add(popupContainer);

        Box background = new();
        background.AddToClassList("fleet-popup-background");
        SetBorderWidthRadius(background, 5, 10);
        SetPadding(background, 50);
        popupContainer.Add(background);

        Label label = new("Unable to enter fleet view during combat!");
        label.AddToClassList("fleet-popup-text");
        SetFontSize(label, 30);
        SetMargin(label, 0, 50, 0, 0);
        background.Add(label);

        Button button = new(OnPopupExit);
        button.AddToClassList("fleet-popup-button");
        SetBorderWidthRadius(button, 3, 7);
        SetPadding(button, 10, 10, 50, 50);
        SetFontSize(button, 36);
        button.text = "Exit";
        background.Add(button);
    }

    private void OnPopupExit()
    {
        UIManager.Instance.SetState(UIState.HUD);
        CameraManager.Instance.SetState(CameraState.Player);
        FirstPersonController.Instance.SetState(PlayerState.FirstPerson);
    }

    private void Draw()
    {
        container.Clear();

        CreateCurrentContainer(container);
        CreateRightColumn(container);
    }

    private void CreateCurrentContainer(VisualElement _parent)
    {
        VisualElement currentContainer = new();
        currentContainer.AddToClassList("fleet-current-container");
        _parent.Add(currentContainer);

        VisualElement resourceRow = new();
        resourceRow.AddToClassList("fleet-current-resource-row");
        SetMargin(resourceRow, 25, 0, 0, 0);
        SetPadding(resourceRow, 6, 6, 30, 30);
        SetBorderRadius(resourceRow, 10);
        currentContainer.Add(resourceRow);

        Image image = new();
        SetSize(image, 64, 64);
        SetMargin(image, 0, 0, 0, 10);
        image.image = resourceIcon;
        resourceRow.Add(image);

        Label resourceLabel = new($"{ResourceManager.Instance.Amount}");
        resourceLabel.AddToClassList("fleet-current-resource-label");
        SetFontSize(resourceLabel, 40);
        resourceRow.Add(resourceLabel);

        VisualElement menuContainer = new();
        menuContainer.AddToClassList("fleet-current-menu-container");
        SetMargin(menuContainer, 0, 25, 0, 0);
        currentContainer.Add(menuContainer);

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
        SetSize(button, 64, 64);
        SetBorderWidthRadius(button, 5, 10);
        SetFontSize(button, 60);
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
        Boat boat = PlayerBoatController.Instance.AdmiralController.Fleet[currentIndex];
        CameraManager.Instance.SetFleetCamera(boat.transform);

        Box container = new();
        container.AddToClassList("fleet-current-boat");
        SetMargin(container, 0, 0, 25, 25);
        SetPadding(container, 20);
        SetBorderWidthRadius(container, 0, 10);
        _parent.Add(container);

        CreateDurabilityRow(container, boat);
        CreateUpgradeButtons(container, boat);
    }

    private void CreateDurabilityRow(VisualElement _parent, Boat _boat)
    {
        VisualElement container = new();
        container.AddToClassList("fleet-current-durability-row");
        SetMargin(container, 0, 15, 0, 0);
        _parent.Add(container);

        Label header = new($"{_boat.Name}");
        header.AddToClassList("fleet-current-header");
        SetFontSize(header, 35);
        container.Add(header);

        Label healthLabel = new($"[{_boat.GetPercentageDurability()}% HP]");
        healthLabel.AddToClassList("fleet-current-header");
        SetFontSize(healthLabel, 22);
        SetMargin(header, 0, 0, 12, 8);
        container.Add(healthLabel);

        if (_boat.IsDamaged)
        {
            Button button = new(() => OnRepair(_boat));
            button.AddToClassList("main-button");
            button.AddToClassList("fleet-current-repair-button");
            SetBorderWidthRadius(button, 3, 7);
            SetPadding(button, 10);
            button.SetEnabled(_boat.IsDamaged && ResourceManager.Instance.CanRepair(_boat));
            container.Add(button);

            Label label = new($"Repair");
            label.AddToClassList("fleet-current-repair-label");
            SetFontSize(label, 22);
            button.Add(label);

            Image image = new();
            SetSize(image, 32, 32);
            image.image = repairIcon;
            button.Add(image);

            CreateUpgradeCostLabel(button, 19, ResourceManager.GetRepairCost(_boat));
        }
    }

    private void OnRepair(Boat _boat)
    {
        ResourceManager.Instance.RepairBoat(_boat);
        HUDScreen.Instance.CompleteObjective(ObjectiveType.RepairShip);
        OnBoatRepaired?.Invoke();
        Draw();
    }

    private void CreateUpgradeButtons(VisualElement _parent, Boat _boat)
    {
        VisualElement container = new();
        container.AddToClassList("fleet-current-upgrade-container");
        _parent.Add(container);

        CreateUpgradeButton(container, _boat, UpgradeType.Hull, () => _boat.Repair());
        CreateUpgradeButton(container, _boat, UpgradeType.Cannons);
    }

    private void CreateUpgradeButton(VisualElement _parent, Boat _boat, UpgradeType _type, Action _action = null)
    {
        VisualElement container = new();
        container.AddToClassList("fleet-current-upgrade");
        SetMargin(container, 0, 0, 10, 0);
        _parent.Add(container);

        Label description = new($"Tier {_boat.GetTierOfUpgradeAsString(_type)} {_type} ({_boat.GetUpgradeModifierPercentage(_type)}% {_boat.GetModifierDescription(_type)})");
        description.AddToClassList("fleet-current-upgrade-description");
        SetFontSize(description, 22);
        container.Add(description);

        Button button = new(() => OnUpgrade(_boat, _type, _action));
        button.AddToClassList("main-button");
        button.AddToClassList("fleet-current-upgrade-button");
        SetBorderWidthRadius(button, 0, 10);
        button.SetEnabled(_boat.CanUpgrade(_type));
        container.Add(button);

        Label label = new(_boat.IsUpgradeMaxed(_type) ? "MAXED" : $"+ {_boat.GetUpgradeIncreasePercentage(_type)}% {_boat.GetModifierDescription(_type)}");
        label.AddToClassList("fleet-current-upgrade-label");
        SetFontSize(label, 22);
        button.Add(label);

        Image image = new();
        SetSize(image, 32, 32);
        image.image = upgradeIcon;
        button.Add(image);

        if (!_boat.IsUpgradeMaxed(_type))
        {
            CreateUpgradeCostLabel(button, 19, ResourceManager.GetUpgradeCost());
        }
    }

    private void OnUpgrade(Boat _boat, UpgradeType _type, Action _action = null)
    {
        HUDScreen.Instance.CompleteObjective(ObjectiveType.UpgradeShip);
        _boat.Upgrade(_type);
        _action?.Invoke();
        OnBoatUpgraded?.Invoke();
        Draw();
    }

    private void CreateRightColumn(VisualElement _parent)
    {
        VisualElement container = new();
        container.AddToClassList("fleet-right-column");
        _parent.Add(container);

        CreateRepairAllButton(container);
        CreateBoatList(container);
        CreateFleetBuildContainer(container);
    }

    private void CreateRepairAllButton(VisualElement _parent)
    {
        Button button = new(() => OnRepairAll());
        button.AddToClassList("main-button");
        button.AddToClassList("fleet-repair-all");
        SetMargin(button, 0, 0, 10, 0);
        SetPadding(button, 10);
        SetBorderWidthRadius(button, 5, 10);
        button.SetEnabled(ResourceManager.Instance.CanRepairAll());
        _parent.Add(button);

        Label label = new($"Repair All");
        label.AddToClassList("fleet-repair-all-label");
        SetFontSize(label, 25);
        button.Add(label);

        Image image = new();
        SetSize(image, 50, 50);
        image.image = repairIcon;
        button.Add(image);

        CreateUpgradeCostLabel(button, 22, ResourceManager.GetRepairAllCost());
    }

    private void OnRepairAll()
    {
        ResourceManager.Instance.RepairAll();
        HUDScreen.Instance.CompleteObjective(ObjectiveType.RepairShip);
        OnBoatRepaired?.Invoke();
        Draw();
    }

    private void CreateBoatList(VisualElement _parent)
    {
        ScrollView boatListContainer = new();
        boatListContainer.AddToClassList("fleet-boat-list-container");
        SetPadding(boatListContainer, 5, 5, 0, 0);
        SetBorderRadius(boatListContainer, 10);
        boatListContainer.verticalScroller.highButton.RemoveFromHierarchy();
        boatListContainer.verticalScroller.lowButton.RemoveFromHierarchy();
        boatListContainer.horizontalScroller.RemoveFromHierarchy();
        _parent.Add(boatListContainer);

        for (int i = 0; i < PlayerBoatController.Instance.AdmiralController.Fleet.Count; i++)
        {
            CreateBoatItem(boatListContainer, PlayerBoatController.Instance.AdmiralController.Fleet[i], i);
        }
    }

    private Button CreateBoatItem(VisualElement _parent, Boat _boat, int _index)
    {
        Button button = new(() => OnBoatItem(_index));
        button.AddToClassList("main-button");
        button.AddToClassList("fleet-boat-list-item");
        SetMargin(button, 5, 5, 10, 10);
        SetBorderWidthRadius(button, 0, 10);
        _parent.Add(button);

        Image image = new();
        SetSize(image, 32, 32);
        image.image = boatIcon;
        button.Add(image);

        Label label = new($"{_boat.Name} [{_boat.GetPercentageDurability()}% HP]");
        label.AddToClassList("fleet-boat-list-item-label");
        SetFontSize(label, 25);
        button.Add(label);

        return button;
    }

    private void OnBoatItem(int _index)
    {
        currentIndex = _index;
        Draw();
    }

    private void CreateFleetBuildContainer(VisualElement _parent)
    {
        VisualElement container = new();
        container.AddToClassList("fleet-build-container");
        SetBorderRadius(container, 10);
        _parent.Add(container);

        Label header = new($"{PlayerBoatController.Instance.AdmiralController.Subordinates.Count}/{PlayerBoatController.Instance.AdmiralController.GetSubordinateCap} Fleet Size");
        header.AddToClassList("fleet-build-header");
        SetFontSize(header, 30);
        container.Add(header);

        CreateBuildButton(container);
        CreateFleetCapUpgradeButton(container, PlayerBoatController.Instance.AdmiralController);
    }

    private void CreateBuildButton(VisualElement _parent)
    {
        Button button = new(() => OnBuild());
        button.AddToClassList("main-button");
        button.AddToClassList("fleet-build-button");
        SetBorderWidthRadius(button, 0, 10);
        SetMargin(button, 0, 20, 0, 0);
        button.SetEnabled(ResourceManager.Instance.CanBuild() && PlayerBoatController.Instance.AdmiralController.CanBuild);
        _parent.Add(button);

        Label label = new($"Build New Boat");
        label.AddToClassList("fleet-build-button-label");
        SetFontSize(label, 25);
        button.Add(label);

        Image image = new();
        SetSize(image, 32, 32);
        image.image = buildIcon;
        button.Add(image);

        CreateUpgradeCostLabel(button, 19, ResourceManager.Instance.GetBuildCost());
    }

    private void OnBuild()
    {
        HUDScreen.Instance.CompleteObjective(ObjectiveType.BuildShip);
        ResourceManager.Instance.BuildPlayerBoat();
        OnBoatBuilt?.Invoke();
        currentIndex = PlayerBoatController.Instance.AdmiralController.Fleet.Count - 1;
        Draw();
    }

    private void CreateFleetCapUpgradeButton(VisualElement _parent, PlayerAdmiralController _admiral)
    {
        Button button = new(() => OnSubordinateCap(_admiral));
        button.AddToClassList("main-button");
        button.AddToClassList("fleet-upgrade-fleet-cap-button");
        SetBorderWidthRadius(button, 0, 10);
        SetMargin(button, 0, 20, 0, 0);
        button.SetEnabled(_admiral.CanUpgradeSubodinateCap);
        _parent.Add(button);

        Label label = new(_admiral.SuborinateUpgradeIndex < PlayerAdmiralController.MAX_SUBORDINATE_UPGRADE ? $"+ {_admiral.GetSubordinateCapIncrease} Fleet Size" : "MAXED");
        label.AddToClassList("fleet-upgrade-fleet-cap-label");
        SetFontSize(label, 22);
        button.Add(label);

        Image image = new();
        SetSize(image, 32, 32);
        image.image = upgradeIcon;
        button.Add(image);

        if (_admiral.SuborinateUpgradeIndex < PlayerAdmiralController.MAX_SUBORDINATE_UPGRADE)
        {
            CreateUpgradeCostLabel(button, 19, _admiral.GetSubordinateUpgradeCost);
        }
    }

    private void OnSubordinateCap(PlayerAdmiralController _admiral)
    {
        _admiral.UpgradeSuborniateCap();
        OnBoatUpgraded?.Invoke();
        Draw();
    }

    private void CreateUpgradeCostLabel(VisualElement _parent, int _size, int _cost)
    {
        VisualElement wrapper = new();
        wrapper.AddToClassList("fleet-upgrade-cost-wrapper");
        _parent.Add(wrapper);

        VisualElement background = new();
        background.AddToClassList("fleet-upgrade-cost-background");
        SetPadding(background, 3, 3, 20, 20);
        SetBorderRadius(background, 10);
        wrapper.Add(background);    

        Image image = new();
        SetSize(image, _size, _size);
        SetMargin(image, 0, 0, 0, 5);
        image.image = resourceIcon;
        background.Add(image);

        Label label = new($"-{_cost}");
        label.AddToClassList("fleet-upgrade-cost-label");
        SetFontSize(label, _size);
        background.Add(label);
    }
}