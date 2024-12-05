using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

public class CommandScreen : UIScreen
{
    public static CommandScreen Instance { get; private set; }

    protected override List<UIState> ActiveStates => new() { UIState.Formation, UIState.HUD };

    public CommandScreenState State { get; private set; } = CommandScreenState.Hidden;
    private float stateTimer = 0;
    private const float TIME_SHOWING = 4f;
    private const float TIME_FADING = 1f;

    private PlayerAdmiralController admiralController;

    private VisualElement buttonContainer;
    private Button changeViewButton;

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;

        UIManager.OnStateChanged += UIManager_OnStateChanged;
    }

    private void Start()
    {
        admiralController = PlayerBoatController.Instance.AdmiralController;
    }

    private void OnDestroy()
    {
        UIManager.OnStateChanged -= UIManager_OnStateChanged;
    }

    private void Update()
    {
        if (UIManager.Instance.State == UIState.Formation)
        {
            Show();
        }

        switch (State)
        {
            case CommandScreenState.Visible:
                ShowingState();
                break;

            case CommandScreenState.Fading:
                FadingState();
                break;

            case CommandScreenState.Hidden:
                HiddenState();
                break;
        }
    }

    public void Show()
    {
        stateTimer = TIME_SHOWING;
        State = CommandScreenState.Visible;
    }

    private void ShowingState()
    {
        if ((stateTimer -= Time.deltaTime) <= 0)
        {
            stateTimer = TIME_FADING;
            State = CommandScreenState.Fading;
        }

        else
        {
            buttonContainer.style.opacity = 1;
        }
    }

    private void FadingState()
    {
        if ((stateTimer -= Time.deltaTime) <= 0)
        {
            State = CommandScreenState.Hidden;
        }

        else
        {
            buttonContainer.style.opacity = stateTimer / TIME_FADING;
        }
    }

    private void HiddenState()
    {
        buttonContainer.style.opacity = 0;
    }

    private void UIManager_OnStateChanged(UIState _state)
    {
        changeViewButton.text = GetChangeViewText();
    }

    public override void Generate()
    {
        VisualElement container = new();
        container.AddToClassList("command-container");
        container.pickingMode = PickingMode.Ignore;
        Root.Add(container);

        buttonContainer = new();
        buttonContainer.AddToClassList("command-button-container");
        buttonContainer.style.opacity = 0;
        buttonContainer.pickingMode = PickingMode.Ignore;
        container.Add(buttonContainer);

        CreateTopButtons(buttonContainer);
        CreateChangeViewButton(buttonContainer);
    }

    private void CreateTopButtons(VisualElement _parent)
    {
        VisualElement container = new();
        container.AddToClassList("command-top-button-container");
        _parent.Add(container);

        CreateTopButton(container, "1: Formation", () => admiralController.SetCommandForSubordinates(Command.Formation));
        CreateTopButton(container, "2: Hold", () => admiralController.SetCommandForSubordinates(Command.Hold));
        CreateTopButton(container, "3: Charge", () => admiralController.SetCommandForSubordinates(Command.Charge));
    }

    private void CreateTopButton(VisualElement _parent, string _name, Action _onClicked)
    {
        Button button = new(_onClicked);
        button.AddToClassList("command-top-button");
        button.pickingMode = PickingMode.Position;
        SetFontSize(button, 22);
        button.text = _name;
        _parent.Add(button);
    }

    private void CreateChangeViewButton(VisualElement _parent)
    {
        changeViewButton = new(OnChangeView);
        changeViewButton.AddToClassList("command-change-view-button");
        changeViewButton.pickingMode = PickingMode.Position;
        SetFontSize(changeViewButton, 26);
        changeViewButton.text = GetChangeViewText();
        _parent.Add(changeViewButton);
    }

    private void OnChangeView()
    {
        if (UIManager.Instance.State == UIState.Formation) UIManager.Instance.ExitFormationView();
        else UIManager.Instance.EnterFormationView();
    }

    private string GetChangeViewText() => $"4: {(UIManager.Instance.State == UIState.Formation ? "Exit" : "Enter")} Formation View";
}

public enum CommandScreenState
{
    Visible,
    Fading,
    Hidden
}