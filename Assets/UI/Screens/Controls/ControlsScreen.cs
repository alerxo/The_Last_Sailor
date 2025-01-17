using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public class ControlsScreen : UIScreen
{
    protected override List<UIState> ActiveStates => new() { UIState.Controls };

    [SerializeField] private Texture2D backgroundImage;
    [SerializeField] private StyleSheet scrollViewStyleSheet;

    public override void Generate()
    { 
        VisualElement container = new();
        container.AddToClassList("options-container");
        Root.Add(container);

        VisualElement background = new();
        background.AddToClassList("options-background");
        SetSize(background, 600, 600);
        SetPadding(background, 50);
        background.style.backgroundImage = backgroundImage;
        container.Add(background);

        Button returnButton = new(() => UIManager.Instance.ReturnFromControls());
        returnButton.AddToClassList("options-return-button");
        SetMargin(returnButton, 7, 25, 0, 0);
        SetFontSize(returnButton, 35);
        returnButton.text = "-Return-";
        background.Add(returnButton);

        ScrollView optionsContainer = new();
        optionsContainer.styleSheets.Add(scrollViewStyleSheet);
        optionsContainer.AddToClassList("options-item-container");
        SetBorderWidthRadius(optionsContainer, 3, 7);
        SetPadding(optionsContainer, 0, 0, 10, 0);
        optionsContainer.verticalScroller.highButton.RemoveFromHierarchy();
        optionsContainer.verticalScroller.lowButton.RemoveFromHierarchy();
        optionsContainer.horizontalScroller.RemoveFromHierarchy();
        background.Add(optionsContainer);

        CreateCategoryHeader(optionsContainer, "General");
        CreateControl(optionsContainer, "Pause", "Esc");
        CreateControl(optionsContainer, "Go Back", "Esc");
        CreateControl(optionsContainer, "Show Objective", "Tab");

        CreateCategoryHeader(optionsContainer, "Movement");
        CreateControl(optionsContainer, "Walk", "W", "A", "S", "D");
        CreateControl(optionsContainer, "Jump", "Space");
        CreateControl(optionsContainer, "Sprint", "LShift");

        CreateCategoryHeader(optionsContainer, "Interactions");
        CreateControl(optionsContainer, "Interact", "E");
        CreateControl(optionsContainer, "Exit Interaction", "E");
        CreateControl(optionsContainer, "Aim Cannon", "W", "A", "S", "D");
        CreateControl(optionsContainer, "Shoot Cannon", "LMB");
        CreateControl(optionsContainer, "Move Throttle", "A", "D");
        CreateControl(optionsContainer, "Turn Steering Wheel", "A", "D");
        CreateControl(optionsContainer, "Steering Camera", "C");

        CreateCategoryHeader(optionsContainer, "Fleet Commands");
        CreateControl(optionsContainer, "Fleet Follow Command", "1");
        CreateControl(optionsContainer, "Fleet Wait Command", "2");
        CreateControl(optionsContainer, "Fleet Charge Command", "3");

        CreateCategoryHeader(optionsContainer, "Formation View");
        CreateControl(optionsContainer, "Enter Formation View", "4");
        CreateControl(optionsContainer, "Move Fleet Camera", "W", "A", "S", "D");
        CreateControl(optionsContainer, "Zoom Fleet Camera", "Scroll");
    }

    private void CreateCategoryHeader(VisualElement _parent, string _header)
    {
        VisualElement container = new();
        container.AddToClassList("options-category");
        SetMargin(container, 20, 10, 0, 0);
        _parent.Add(container);

        Label label = new(_header);
        label.AddToClassList("options-category-header");
        SetFontSize(label, 32);
        container.Add(label);

        Box line = new();
        line.AddToClassList("options-category-header-line");
        SetHeight(line, 3);
        container.Add(line);
    }

    private VisualElement CreateItem(VisualElement _parent)
    {
        VisualElement container = new();
        container.AddToClassList("options-item");
        _parent.Add(container);

        return container;
    }

    private void CreateItemLabel(VisualElement _parent, string _text)
    {
        Label label = new(_text);
        label.AddToClassList("options-item-label");
        SetFontSize(label, 20);
        _parent.Add(label);
    }

    private void CreateControl(VisualElement _parent, string _name, params string[] _controlScheme)
    {
        VisualElement container = CreateItem(_parent);

        CreateItemLabel(container, _name);

        VisualElement controls = new();
        controls.AddToClassList("options-item-control-container");
        container.Add(controls);

        foreach (string control in _controlScheme)
        {
            Label inputLabel = new(control);
            inputLabel.AddToClassList("options-item-control");
            SetPadding(inputLabel, 0, 0, 10, 10);
            SetBorderWidthRadius(inputLabel, 3, 5);
            SetFontSize(inputLabel, 20);
            controls.Add(inputLabel);
        }
    }
}
