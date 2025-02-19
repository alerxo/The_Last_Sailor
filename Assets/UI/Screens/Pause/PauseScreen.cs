using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PauseScreen : UIScreen
{
    protected override List<UIState> ActiveStates => new() { UIState.Pause };

    [SerializeField] private Texture2D backgroundImage;

    public override void Generate()
    {
        VisualElement container = new();
        container.AddToClassList("pause-container");
        Root.Add(container);

        VisualElement background = new();
        background.AddToClassList("pause-background");
        SetSize(background, 600, 600);
        SetPadding(background, 100, 100, 150, 150);
        background.style.backgroundImage = backgroundImage;
        container.Add(background);

        Label header = new("PAUSED");
        header.AddToClassList("pause-header");
        SetMargin(header, 0, 35, 0, 0);
        SetFontSize(header, 55);
        background.Add(header);

        VisualElement buttons = new();
        buttons.AddToClassList("pause-button-container");
        background.Add(buttons);

        CreateButton(buttons, "-Resume-", () => UIManager.Instance.SetState(UIState.HUD));
        CreateButton(buttons, "-Options-", () => UIManager.Instance.SetStateOptions(UIState.Pause));
        CreateButton(buttons, "-Controls-", () => UIManager.Instance.SetStateControls(UIState.Pause));
        CreateButton(buttons, "-Main Menu-", OnMainMenu);
    }

    private void CreateButton(VisualElement _parent, string _text, Action _action)
    {
        Button button = new(_action);
        button.AddToClassList("pause-button");
        SetMargin(button, 7, 0, 0, 0);
        SetFontSize(button, 38);
        button.text = _text;
        button.clicked += UIManager.InvokeOnUIButtonClicked;
        button.RegisterCallback<MouseEnterEvent>(evt => UIManager.InvokeOnUIButtonHovered());
        _parent.Add(button);
    }

    private void OnMainMenu()
    {
        UIManager.Instance.SetState(UIState.TitleScreen);
        SceneManager.LoadScene("Game");
    }
}
