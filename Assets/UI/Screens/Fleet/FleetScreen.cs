using UnityEngine;
using UnityEngine.UIElements;

public class FleetScreen : UIScreen
{
    protected override UIState ActiveState => UIState.Fleet;

    private int currentBoat;

    private Box boatContainer;

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
        if (_state == UIState.Fleet)
        {
            currentBoat = 0;
            FillBoatContainer();
        }
    }

    public override void Generate()
    {
        VisualElement container = new();
        container.AddToClassList("fleet-container");
        root.Add(container);

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

        Boat boat = PlayerBoatController.Instance.AdmiralController.Fleet[currentBoat];

        Label header = new(boat.Name);
        header.AddToClassList("fleet-boat-header");
        SetFontSize(header, 40);
        boatContainer.Add(header);

        VisualElement buttonContainer = new();
        buttonContainer.AddToClassList("fleet-boat-button-container");
        boatContainer.Add(buttonContainer);

        CreateBoatButton(buttonContainer, "Upgrade Hull", boat, UpgradeType.Hull);
        CreateBoatButton(buttonContainer, "Upgrade Cannons", boat, UpgradeType.Cannons);
        CreateBoatButton(buttonContainer, "Upgrade Engine", boat, UpgradeType.Engine);
    }

    private void CreateBoatButton(VisualElement _parent, string _text, Boat _boat, UpgradeType _type)
    {
        Button button = new(() => _boat.Upgrade(_type));
        button.AddToClassList("main-button");
        button.AddToClassList("fleet-boat-button");
        SetFontSize(button, 25);
        button.text = _text;
        button.SetEnabled(_boat.CanUpgrade(_type));
        _parent.Add(button);
    }

    private void CreateNavigatioButton(VisualElement _parent, string _text, int _index)
    {
        Button button = new(() => OnNavigationArrow(_index));
        button.AddToClassList("main-button");
        button.AddToClassList("fleet-navigation-arrow-button");
        SetFontSize(button, 30);
        button.text = _text;
        _parent.Add(button);
    }

    public void OnNavigationArrow(int _index)
    {
        currentBoat += _index;

        if (currentBoat < 0) currentBoat = PlayerBoatController.Instance.AdmiralController.Fleet.Count - 1;
        else if (currentBoat > PlayerBoatController.Instance.AdmiralController.Fleet.Count - 1) currentBoat = 0;

        FillBoatContainer();
    }
}
