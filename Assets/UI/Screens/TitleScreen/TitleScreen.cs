using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TitleScreen : UIScreen
{
    protected override List<UIState> ActiveStates => new() { UIState.TitleScreen };

    [SerializeField] private Texture2D backgroundImage;

    public override void Generate()
    {
        VisualElement container = new();
        container.AddToClassList("title-screen-container");
        Root.Add(container);

        VisualElement background = new();
        background.AddToClassList("title-screen-background");
        SetMargin(background, 125, 0, 100, 0);
        SetPadding(background, 100, 100, 130, 130);
        background.style.backgroundImage = backgroundImage;
        container.Add(background);

        Label title = new("The Last Sailor");
        title.AddToClassList("title-screen-title");
        SetMargin(title, 0, 30, 0, 0);
        SetFontSize(title, 60);
        background.Add(title);

        VisualElement buttons = new();
        buttons.AddToClassList("title-screen-button-container");
        background.Add(buttons);

        CreateButton(buttons, "-Play-", OnPlay);
        CreateButton(buttons, "-Options-", () => UIManager.Instance.SetStateOptions(UIState.TitleScreen));
        CreateButton(buttons, "-Controls-", () => UIManager.Instance.SetStateControls(UIState.TitleScreen));
        CreateButton(buttons, "-Credits-", () => UIManager.Instance.SetState(UIState.Credits));
        CreateButton(buttons, "-Quit-", () => Application.Quit());
    }

    private void CreateButton(VisualElement _parent, string _text, Action _action)
    {
        Button button = new(_action);
        button.AddToClassList("main-button");
        button.AddToClassList("title-screen-button");
        SetMargin(button, 2, 0, 0, 0);
        SetFontSize(button, 32);
        button.text = _text;
        button.clicked += UIManager.InvokeOnUIButtonClicked;
        button.RegisterCallback<MouseEnterEvent>(evt => UIManager.InvokeOnUIButtonHovered());
        _parent.Add(button);
    }

    private void OnPlay()
    {
        UIManager.Instance.SetState(UIState.HUD);
        CameraManager.Instance.SetState(CameraState.Player);
        FirstPersonController.Instance.SetState(PlayerState.FirstPerson);
    }
}
