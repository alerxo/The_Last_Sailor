using System;
using UnityEngine;
using UnityEngine.UIElements;

public class TitleScreen : UIScreen
{
    protected override UIState ActiveState => UIState.MainMenu;
    [SerializeField] private Texture2D backgroundImage;

    protected override void Generate()
    {
        Image background = new();
        background.AddToClassList("title-screen-background");
        background.image = backgroundImage;
        document.rootVisualElement.Add(background);

        Label title = new("Steamboat\nFleet Builder");
        title.AddToClassList("title-screen-title");
        background.Add(title);

        VisualElement buttons = new();
        buttons.AddToClassList("title-screen-button-container");
        background.Add(buttons);

        CreateButton(buttons, "-Play-", OnPlay);
        CreateButton(buttons, "-Options-", () => throw new NotImplementedException());
        CreateButton(buttons, "-Quit-", () => Application.Quit());
    }

    private static void OnPlay()
    {
        UIManager.Instance.SetState(UIState.Game);
        CameraManager.Instance.SetState(CameraState.Player);
    }

    private void CreateButton(VisualElement parent, string text, Action action)
    {
        Button button = new(action);
        button.AddToClassList("title-screen-button");
        button.text = text;
        parent.Add(button);
    }
}
