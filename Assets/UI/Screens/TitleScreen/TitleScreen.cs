using System;
using UnityEngine;
using UnityEngine.UIElements;

public class TitleScreen : UIScreen
{
    protected override UIState ActiveState => UIState.TitleScreen;
    [SerializeField] private Texture2D backgroundImage;

    public override void Generate()
    {
        Image background = new();
        background.AddToClassList("title-screen-background");
        background.image = backgroundImage;
        root.Add(background);

        Label title = new("The Last Sailor");
        title.AddToClassList("title-screen-title");
        SetFontSize(title, 60);
        background.Add(title);

        VisualElement buttons = new();
        buttons.AddToClassList("title-screen-button-container");
        background.Add(buttons);

        CreateButton(buttons, "-Play-", OnPlay);
        CreateButton(buttons, "-Options-", () => UIManager.Instance.SetStateOptions(UIState.TitleScreen));
        CreateButton(buttons, "-Quit-", () => Application.Quit());
    }

    private void CreateButton(VisualElement _parent, string _text, Action _action)
    {
        Button button = new(_action);
        button.AddToClassList("title-screen-button");
        SetFontSize(button, 35);
        button.text = _text;
        _parent.Add(button);
    }

    private static void OnPlay()
    {
        UIManager.Instance.SetState(UIState.HUD);
        CameraManager.Instance.SetState(CameraState.Player);
        FirstPersonController.Instance.SetState(PlayerState.FirstPerson);
    }
}
