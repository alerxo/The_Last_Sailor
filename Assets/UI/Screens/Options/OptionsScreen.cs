using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class OptionsScreen : UIScreen
{
    protected override List<UIState> ActiveStates => new() { UIState.Options };

    [SerializeField] private Texture2D backgroundImage;
    [SerializeField] private StyleSheet sliderStyleSheet, scrollViewStyleSheet;

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

        Button returnButton = new(() => UIManager.Instance.ReturnFromOptions());
        returnButton.AddToClassList("options-return-button");
        SetMargin(returnButton, 7, 25, 0, 0);
        SetFontSize(returnButton, 35);
        returnButton.text = "-Return-";
        background.Add(returnButton);

        ScrollView optionsContainer = new();
        optionsContainer.styleSheets.Add(scrollViewStyleSheet);
        optionsContainer.AddToClassList("options-item-container");
        SetBorderWidthRadius(optionsContainer, 3, 7);
        SetPadding(optionsContainer, 0, 0, 5, 0);
        optionsContainer.verticalScroller.highButton.RemoveFromHierarchy();
        optionsContainer.verticalScroller.lowButton.RemoveFromHierarchy();
        optionsContainer.horizontalScroller.RemoveFromHierarchy();
        background.Add(optionsContainer);

        CreateCategoryHeader(optionsContainer, "Video");
        CreateDropDown(optionsContainer, "Quality", new List<string>() { "Low", "Medium", "High" }, (int)VideoQualityManager.Instance.VideoQuality, (i) => VideoQualityManager.Instance.SetVideoQuality((VideoQuality)i));

        CreateCategoryHeader(optionsContainer, "Volume");
        CreateSlider(optionsContainer, "Master", SoundSettingsManager.Instance.GetMasterVolume(), (f) => SoundSettingsManager.Instance.SetMasterVolume(f));
        CreateSlider(optionsContainer, "Music", SoundSettingsManager.Instance.GetMusicVolume(), (f) => SoundSettingsManager.Instance.SetMusicVolume(f));
        CreateSlider(optionsContainer, "Sound Effects", SoundSettingsManager.Instance.GetSFXVolume(), (f) => SoundSettingsManager.Instance.SetSFXVolume(f));
        CreateSlider(optionsContainer, "Ambience", SoundSettingsManager.Instance.GetAmbianceVolume(), (f) => SoundSettingsManager.Instance.SetAmbianceVolume(f));

        CreateCategoryHeader(optionsContainer, "Controls");
        CreateControl(optionsContainer, "Pause", false, "Esc");
        CreateControl(optionsContainer, "Go Back", false, "Esc");
        CreateControl(optionsContainer, "Show Objective", true, "Tab");

        CreateControl(optionsContainer, "Walk", false, "W", "A", "S", "D");
        CreateControl(optionsContainer, "Jump", false, "Space");
        CreateControl(optionsContainer, "Sprint", true,  "LShift");

        CreateControl(optionsContainer, "Interact", false, "E");
        CreateControl(optionsContainer, "Exit Interaction", false, "E");
        CreateControl(optionsContainer, "Aim Cannon", false, "W", "A", "S", "D");
        CreateControl(optionsContainer, "Shoot Cannon", true, "LMB");

        CreateControl(optionsContainer, "Throttle", false, "A", "D");
        CreateControl(optionsContainer, "Steering Wheel", false, "A", "D");
        CreateControl(optionsContainer, "Steering Camera", true, "C");

        CreateControl(optionsContainer, "Fleet Follow Command", false, "1");
        CreateControl(optionsContainer, "Fleet Wait Command", false, "2");
        CreateControl(optionsContainer, "Fleet Charge Command", false, "3");
        CreateControl(optionsContainer, "Enter Fleet View", true, "4");

        CreateControl(optionsContainer, "Move Fleet Camera", false, "W", "A", "S", "D");
        CreateControl(optionsContainer, "Zoom Fleet Camera", true, "Scroll");
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

    private void CreateDropDown(VisualElement _parent, string _name, List<string> _choices, int _current, Action<int> _onSet)
    {
        VisualElement container = CreateItem(_parent);

        CreateItemLabel(container, _name);

        DropdownField dropdownField = new(_choices, _current);
        dropdownField.AddToClassList("options-item-dropdown");
        dropdownField.RegisterValueChangedCallback((e) => _onSet(_choices.IndexOf(e.newValue)));
        container.Add(dropdownField);
    }

    private void CreateSlider(VisualElement _parent, string _name, float _current, Action<float> _onSet)
    {
        VisualElement container = CreateItem(_parent);

        CreateItemLabel(container, _name);

        VisualElement sliderContainer = new();
        sliderContainer.AddToClassList("options-item-slider-container");
        container.Add(sliderContainer);

        Slider slider = new(0f, 100f, SliderDirection.Horizontal);
        slider.styleSheets.Add(sliderStyleSheet);
        slider.AddToClassList("options-item-slider");
        slider.SetValueWithoutNotify(_current);
        slider.RegisterValueChangedCallback(evt => _onSet(evt.newValue));
        sliderContainer.Add(slider);
    }

    private void CreateControl(VisualElement _parent, string _name, bool _isLast, params string[] _controlScheme)
    {
        VisualElement container = CreateItem(_parent);

        if (_isLast)
        {
            SetMargin(container, 0, 25, 0, 0);
        }

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
