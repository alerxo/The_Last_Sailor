using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PauseScreen : UIScreen
{
    protected override UIState ActiveState => UIState.Pause;

    private void Awake()
    {
        UIManager.OnStateChanged += UIManager_OnStateChanged;
    }

    private void OnDestroy()
    {
        UIManager.OnStateChanged -= UIManager_OnStateChanged;
    }

    private void UIManager_OnStateChanged(UIState _state)
    {
        Time.timeScale = _state == UIState.Pause ? 0 : 1;
    }

    protected override void Generate()
    {
        VisualElement container = new();
        container.AddToClassList("pause-container");
        root.Add(container);

        Label header = new("PAUSED");
        header.AddToClassList("pause-header");
        SetFontSize(header, 60);
        container.Add(header);

        VisualElement buttons = new();
        buttons.AddToClassList("pause-button-container");
        container.Add(buttons);

        CreateButton(buttons, "-Resume-", () => UIManager.Instance.SetState(UIState.HUD));
        CreateButton(buttons, "-Options-", () => throw new NotImplementedException());
        CreateButton(buttons, "-Main Menu-", OnMainMenu);
    }

    private void CreateButton(VisualElement _parent, string _text, Action _action)
    {
        Button button = new(_action);
        button.AddToClassList("pause-button");
        SetWidth(button, 200);
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
