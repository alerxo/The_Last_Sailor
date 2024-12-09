using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class FleetScreen : UIScreen
{
    public static event UnityAction OnBoatUpgraded;
    protected override List<UIState> ActiveStates => new() { UIState.Fleet };

    private int currentBoat;

    private Box boatContainer;
    private readonly List<VisualElement> navigationButtons = new();

    private void Awake()
    {
        UIManager.OnStateChanged += UIManager_OnStateChanged;
        ResourceManager.OnResourceAmountChanged += ResourceManager_OnResourceAmountChanged;
    }

    private void OnDestroy()
    {
        UIManager.OnStateChanged -= UIManager_OnStateChanged;
        ResourceManager.OnResourceAmountChanged -= ResourceManager_OnResourceAmountChanged;
    }

    private void ResourceManager_OnResourceAmountChanged(float _amount)
    {
        if (boatContainer != null && UIManager.Instance.State == UIState.Fleet)
        {
            FillBoatContainer();
        }
    }

    private void UIManager_OnStateChanged(UIState _state)
    {
        if (boatContainer != null && _state == UIState.Fleet)
        {
            currentBoat = 0;
            FillBoatContainer();
        }
    }

    public override void Generate()
    {
        VisualElement container = new();
        container.AddToClassList("fleet-container");
        Root.Add(container);

        VisualElement menuContainer = new();
        menuContainer.AddToClassList("fleet-menu-container");
        container.Add(menuContainer);

        CreateNavigatioButton(menuContainer, "<", -1);
        CreateBoatContainer(menuContainer);
        CreateNavigatioButton(menuContainer, ">", 1);

        CreateBuildButton(menuContainer);
    }

    private void CreateBuildButton(VisualElement _parent)
    {
        Button buildBoatButton = new(() => OnBuild());
        buildBoatButton.AddToClassList("main-button");
        buildBoatButton.AddToClassList("fleet-build-button");
        buildBoatButton.text = "Build New Boat";
        buildBoatButton.SetEnabled(ResourceManager.Instance.CanBuild());
        _parent.Add(buildBoatButton);
    }

    private void OnBuild()
    {
        PlayerBoatController.Instance.AdmiralController.BuildBoat();
        ResourceManager.Instance.BoatWasBuilt();
    }

    private void CreateBoatContainer(VisualElement _parent)
    {
        boatContainer = new();
        boatContainer.AddToClassList("fleet-boat-container");
        _parent.Add(boatContainer);
    }

    private void FillBoatContainer()
    {
        boatContainer.Clear();

        foreach (VisualElement item in navigationButtons)
        {
            item.style.display = PlayerBoatController.Instance.AdmiralController.Fleet.Count > 1 ? DisplayStyle.Flex : DisplayStyle.None;
        }

        Boat boat = PlayerBoatController.Instance.AdmiralController.Fleet[currentBoat];
        CameraManager.Instance.SetFleetCamera(boat.transform);

        CreateTopRow(boat);
        CreateDurabilityContainer(boat);

        VisualElement buttonContainer = new();
        buttonContainer.AddToClassList("fleet-boat-button-container");
        boatContainer.Add(buttonContainer);

        CreateUpgradeButton(buttonContainer, boat, UpgradeType.Hull).clicked += () => boat.Repair();
        CreateUpgradeButton(buttonContainer, boat, UpgradeType.Cannons);
        CreateUpgradeButton(buttonContainer, boat, UpgradeType.Engine);
    }

    private void CreateTopRow(Boat _boat)
    {
        VisualElement topRow = new();
        topRow.AddToClassList("fleet-boat-header-row");
        boatContainer.Add(topRow);

        Label resourceLabel = new($"Resources: {ResourceManager.Instance.Amount}");
        resourceLabel.AddToClassList("fleet-resource-label");
        SetFontSize(resourceLabel, 30);
        topRow.Add(resourceLabel);

        Label header = new(_boat.Name);
        header.AddToClassList("fleet-boat-header");
        SetFontSize(header, 40);
        topRow.Add(header);

        VisualElement exitButtonContainer = new();
        exitButtonContainer.AddToClassList("fleet-boat-exit-button");
        topRow.Add(exitButtonContainer);

        Button exitButton = new(() => OnExit());
        exitButton.AddToClassList("fleet-exit-button");
        SetFontSize(exitButton, 40);
        exitButton.text = "X";
        exitButtonContainer.Add(exitButton);
    }

    private void OnExit()
    {
        UIManager.Instance.SetState(UIState.HUD);
        CameraManager.Instance.SetState(CameraState.Player);
        FirstPersonController.Instance.SetState(PlayerState.FirstPerson);
    }

    private void CreateDurabilityContainer(Boat _boat)
    {
        VisualElement durabilityContainer = new();
        durabilityContainer.AddToClassList("fleet-boat-durability-container");
        boatContainer.Add(durabilityContainer);

        Label durability = new($"Durability: {_boat.GetPercentageHealth()}");
        durability.AddToClassList("fleet-boat-durability-label");
        SetFontSize(durability, 30);
        durabilityContainer.Add(durability);

        Button repairButton = new(() => OnRepair(_boat));
        repairButton.AddToClassList("main-button");
        repairButton.AddToClassList("fleet-boat-repair-button");
        SetFontSize(durability, 28);
        repairButton.text = "Repair (-10 R)";
        repairButton.SetEnabled(_boat.IsDamaged && ResourceManager.Instance.CanRepair(_boat));
        durabilityContainer.Add(repairButton);
    }

    private void OnRepair(Boat _boat)
    {
        _boat.Repair();
        ResourceManager.Instance.BoatWasRepaired(_boat);
    }

    private Button CreateUpgradeButton(VisualElement _parent, Boat _boat, UpgradeType _type)
    {
        VisualElement container = new();
        container.AddToClassList("fleet-boat-upgrade-container");
        _parent.Add(container);

        Label description = new($"Tier {_boat.GetTierOfUpgradeAsString(_type)} {_type} ({_boat.GetUpgradeModifierPercentage(_type)}% {_boat.GetModifierDescription(_type)})");
        description.AddToClassList("fleet-boat-upgrade-description");
        SetFontSize(description, 22);
        container.Add(description);

        Button button = new(() => OnUpgrade(_boat, _type));
        button.AddToClassList("main-button");
        button.AddToClassList("fleet-boat-button");
        SetFontSize(button, 22);
        button.text = $"+ {_boat.GetUpgradeIncreasePercentage(_type)}% {_boat.GetModifierDescription(_type)} (-{Boat.UPGRADE_COST} R)";
        button.SetEnabled(_boat.CanUpgrade(_type));
        container.Add(button);

        return button;
    }

    private static void OnUpgrade(Boat _boat, UpgradeType _type)
    {
        _boat.Upgrade(_type);
        OnBoatUpgraded?.Invoke();
    }

    private void CreateNavigatioButton(VisualElement _parent, string _text, int _index)
    {
        Button button = new(() => OnNavigationArrow(_index));
        button.AddToClassList("main-button");
        button.AddToClassList("fleet-navigation-arrow-button");
        SetFontSize(button, 40);
        button.text = _text;
        _parent.Add(button);

        navigationButtons.Add(button);
    }

    public void OnNavigationArrow(int _index)
    {
        currentBoat += _index;

        if (currentBoat < 0) currentBoat = PlayerBoatController.Instance.AdmiralController.Fleet.Count - 1;
        else if (currentBoat > PlayerBoatController.Instance.AdmiralController.Fleet.Count - 1) currentBoat = 0;

        FillBoatContainer();
    }


}
