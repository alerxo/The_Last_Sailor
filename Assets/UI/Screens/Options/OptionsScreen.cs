using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class OptionsScreen : UIScreen
{
    protected override List<UIState> ActiveStates => new() { UIState.Options };

    [SerializeField] private Texture2D backgroundImage;

    public override void Generate()
    {
        VisualElement container = new();
        container.AddToClassList("options-container");
        Root.Add(container);

        VisualElement background = new();
        background.AddToClassList("options-background");
        SetSize(background, 503, 521);
        SetPadding(background, 50);
        background.style.backgroundImage = backgroundImage;
        container.Add(background);

        Button returnButton = new(() => UIManager.Instance.ReturnFromOptions());
        returnButton.AddToClassList("options-return-button");
        SetMargin(returnButton, 7, 0, 0, 0);
        SetFontSize(returnButton, 35);
        returnButton.text = "-Return-";
        background.Add(returnButton);

        CreateSlider(background, "Master Volume", SoundSettingsManager.Instance.GetMasterVolume(), (f) => SoundSettingsManager.Instance.SetMasterVolume(f));
        CreateSlider(background, "Music Volume", SoundSettingsManager.Instance.GetMusicVolume(), (f) => SoundSettingsManager.Instance.SetMusicVolume(f));
        CreateSlider(background, "Sound Effects Volume", SoundSettingsManager.Instance.GetSFXVolume(), (f) => SoundSettingsManager.Instance.SetSFXVolume(f));
        CreateSlider(background, "Ambience Volume", SoundSettingsManager.Instance.GetAmbianceVolume(), (f) => SoundSettingsManager.Instance.SetAmbianceVolume(f));
    }

    private VisualElement CreateItemContainer(VisualElement _parent)
    {
        VisualElement container = new();
        container.AddToClassList("options-item-container");
        _parent.Add(container);

        return container;
    }

    private void CreateItemLabel(VisualElement _parent, string _text)
    {
        Label label = new(_text);
        label.AddToClassList("options-item-label");
        SetFontSize(label, 24);
        _parent.Add(label);
    }

    private void CreateSlider(VisualElement _parent, string _name, float _current, Action<float> _onSet)
    {
        VisualElement container = CreateItemContainer(_parent);

        CreateItemLabel(container, _name);

        VisualElement sliderContainer = new();
        sliderContainer.AddToClassList("options-item-slider-container");
        container.Add(sliderContainer);

        Slider slider = new(0f, 100f, SliderDirection.Horizontal);
        slider.AddToClassList("options-item-slider");
        slider.SetValueWithoutNotify(_current);
        slider.RegisterValueChangedCallback(evt => _onSet(evt.newValue));
        sliderContainer.Add(slider);
    }
}
