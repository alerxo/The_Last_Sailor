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
        SetPadding(optionsContainer, 0, 0, 10, 0);
        optionsContainer.verticalScroller.highButton.RemoveFromHierarchy();
        optionsContainer.verticalScroller.lowButton.RemoveFromHierarchy();
        optionsContainer.horizontalScroller.RemoveFromHierarchy();
        background.Add(optionsContainer);

        CreateCategoryHeader(optionsContainer, "Video");
        CreateDropDown(optionsContainer, "Quality", new List<string>() { "High", "Low" }, (int)VideoQualityManager.Instance.VideoQuality, (i) => VideoQualityManager.Instance.SetVideoQuality((VideoQuality)i));

        CreateCategoryHeader(optionsContainer, "Volume");
        CreateSlider(optionsContainer, "Master", SoundSettingsManager.Instance.GetMasterVolume(), (f) => SoundSettingsManager.Instance.SetMasterVolume(f));
        CreateSlider(optionsContainer, "Music", SoundSettingsManager.Instance.GetMusicVolume(), (f) => SoundSettingsManager.Instance.SetMusicVolume(f));
        CreateSlider(optionsContainer, "Sound Effects", SoundSettingsManager.Instance.GetSFXVolume(), (f) => SoundSettingsManager.Instance.SetSFXVolume(f));
        CreateSlider(optionsContainer, "Ambience", SoundSettingsManager.Instance.GetAmbianceVolume(), (f) => SoundSettingsManager.Instance.SetAmbianceVolume(f));
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
}
