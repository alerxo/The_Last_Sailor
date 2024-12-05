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
        root.Add(container);

        Image background = new();
        background.AddToClassList("pause-background");
        background.image = backgroundImage;
        container.Add(background);

        Label header = new("PAUSED");
        header.AddToClassList("pause-header");
        SetFontSize(header, 50);
        background.Add(header);

        VisualElement buttons = new();
        buttons.AddToClassList("pause-button-container");
        background.Add(buttons);

        CreateButton(buttons, "-Resume-", () => UIManager.Instance.SetState(UIState.HUD));
        CreateButton(buttons, "-Options-", () => UIManager.Instance.SetStateOptions(UIState.Pause));
        CreateButton(buttons, "-Main Menu-", OnMainMenu);
    }

    private void CreateButton(VisualElement _parent, string _text, Action _action)
    {
        Button button = new(_action);
        button.AddToClassList("pause-button");
        SetFontSize(button, 35);
        button.text = _text;
        _parent.Add(button);
    }

    private void OnMainMenu()
    {
        UIManager.Instance.SetState(UIState.TitleScreen);
        SceneManager.LoadScene("Game");
    }
}
