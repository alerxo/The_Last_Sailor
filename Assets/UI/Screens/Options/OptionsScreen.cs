using System;
using UnityEngine;
using UnityEngine.UIElements;

public class OptionsScreen : UIScreen
{
    protected override UIState ActiveState => UIState.Options;

    public override void Generate()
    {
        VisualElement container = new();
        container.AddToClassList("options-container");
        root.Add(container);

        Box background = new();
        background.AddToClassList("options-background");
        container.Add(background);

        Button returnButton = new(() => UIManager.Instance.ReturnFromOptions());
        returnButton.AddToClassList("options-return-button");
        SetFontSize(returnButton, 22);
        returnButton.text = "Return";
        background.Add(returnButton);

        CreateSlider(background, "Master Volume", 50f, (f) => Debug.LogWarning("Missing Slider Action"));
        CreateSlider(background, "Music Volume", 50f, (f) => Debug.LogWarning("Missing Slider Action"));
        CreateSlider(background, "Sound Effects Volume", 50f, (f) => Debug.LogWarning("Missing Slider Action"));
        CreateSlider(background, "Ambience Volume", 50f, (f) => Debug.LogWarning("Missing Slider Action"));
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
        SetFontSize(label, 22);
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
