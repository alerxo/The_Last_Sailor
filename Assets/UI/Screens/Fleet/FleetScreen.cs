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

        CreateExitButton(boatContainer);

        Boat boat = PlayerBoatController.Instance.AdmiralController.Fleet[currentBoat];
        CameraManager.Instance.SetFleetCamera(boat.transform);

        Label header = new(boat.Name);
        header.AddToClassList("fleet-boat-header");
        SetFontSize(header, 40);
        boatContainer.Add(header);

        Label resourceCount = new($"Resources: {ResourceManager.Instance.Amount}");
        resourceCount.AddToClassList("fleet-resource-count");
        SetFontSize(resourceCount, 30);
        boatContainer.Add(resourceCount);

        VisualElement buttonContainer = new();
        buttonContainer.AddToClassList("fleet-boat-button-container");
        boatContainer.Add(buttonContainer);

        CreateUpgradeButton(buttonContainer, boat, UpgradeType.Hull).clicked += () => boat.Repair();
        CreateUpgradeButton(buttonContainer, boat, UpgradeType.Cannons);
        CreateUpgradeButton(buttonContainer, boat, UpgradeType.Engine);
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

    private void CreateExitButton(VisualElement _parent)
    {
        VisualElement container = new();
        container.AddToClassList("fleet-exit-button-container");
        _parent.Add(container);

        Button button = new(() => OnExit());
        button.AddToClassList("fleet-exit-button");
        SetFontSize(button, 40);
        button.text = "X";
        container.Add(button);
    }

    private void OnExit()
    {
        UIManager.Instance.SetState(UIState.HUD);
        CameraManager.Instance.SetState(CameraState.Player);
        FirstPersonController.Instance.SetState(PlayerState.FirstPerson);
    }
}
