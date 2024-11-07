using System;
using UnityEngine;
using UnityEngine.UIElements;

public class TitleScreen : UIScreen
{
    protected override UIState ActiveState => UIState.TitleScreen;
    [SerializeField] private Texture2D backgroundImage;

    protected override void Generate()
    {
        Image background = new();
        background.AddToClassList("title-screen-background");
        background.image = backgroundImage;
        root.Add(background);

        Label title = new("Steamboat\nFleet Builder");
        title.AddToClassList("title-screen-title");
        SetMargin(title, 20, 0, 0, 0);
        SetFontSize(title, 50);
        background.Add(title);

        VisualElement buttons = new();
        buttons.AddToClassList("title-screen-button-container");
        SetMargin(buttons, 0, 20, 0, 0);
        background.Add(buttons);

        CreateButton(buttons, "-Play-", OnPlay);
        CreateButton(buttons, "-Options-", () => throw new NotImplementedException());
        CreateButton(buttons, "-Quit-", () => Application.Quit());
    }

    private static void OnPlay()
    {
        UIManager.Instance.SetState(UIState.HUD);
        CameraManager.Instance.SetState(CameraState.Player);
    }

    private void CreateButton(VisualElement _parent, string _text, Action _action)
    {
        Button button = new(_action);
        button.AddToClassList("title-screen-button");
        SetWidth(button, 200);
        SetFontSize(button, 35);
        button.text = _text;
        _parent.Add(button);
    }
}
