using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CreditsScreen : UIScreen
{
    protected override List<UIState> ActiveStates => new() { UIState.Credits };

    private ScrollView scrollView;

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
        StopAllCoroutines();

        if (_state == UIState.Credits)
        {
            StartCoroutine(CreditScroll());
        }
    }

    public override void Generate()
    {
        VisualElement container = new();
        container.AddToClassList("credits-background");
        Root.Add(container);

        scrollView = new();
        scrollView.AddToClassList("options-item-container");
        scrollView.RegisterCallback<WheelEvent>((e) => e.StopPropagation(), TrickleDown.TrickleDown);
        scrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
        scrollView.horizontalScroller.RemoveFromHierarchy();
        container.Add(scrollView);

        CreateBuffer();

        CreateCredit(scrollView, "Programmer", "Alexander Robinson");
        CreateCredit(scrollView, "UI & UX Designer", "Alexander Robinson");
        CreateCredit(scrollView, "3D Artist", "Viktor Eriksson");
        CreateCredit(scrollView, "Sound Designer", "Jacob Nordenvall");

        CreateBuffer();
    }

    private void CreateBuffer()
    {
        VisualElement buffer = new();
        buffer.AddToClassList("options-buffer");
        SetHeight(buffer, 1080f);
        scrollView.Add(buffer);
    }

    private void CreateCredit(VisualElement _parent, string _header, params string[] _content)
    {
        VisualElement container = new();
        container.AddToClassList("credits-item");
        SetMargin(container, 20, 20, 0, 0);
        _parent.Add(container);

        Label header = new(_header);
        header.AddToClassList("credit-item-header");
        SetFontSize(header, 60);
        SetMargin(header, 0, 10, 0, 0);
        SetWidth(header, 400);
        container.Add(header);

        foreach (string credit in _content)
        {
            Label label = new(credit);
            label.AddToClassList("credit-item-text");
            SetFontSize(label, 40);
            SetMargin(label, 0, 10, 0, 0);
            SetWidth(label, 300);
            container.Add(label);
        }
    }

    private IEnumerator CreditScroll()
    {
        float timer = 0;
        const float bufferDuration = 1;
        const float mainDuration = 30;

        scrollView.verticalScroller.value = scrollView.verticalScroller.lowValue;

        while ((timer += Time.unscaledDeltaTime) < bufferDuration)
        {
            if (Input.anyKey)
            {
                scrollView.verticalScroller.value = scrollView.verticalScroller.highValue;
                UIManager.Instance.SetState(UIState.TitleScreen);

                yield break;
            }

            yield return null;
        }

        timer = 0;

        while ((timer += Time.unscaledDeltaTime) < mainDuration)
        {
            if (Input.anyKey)
            {
                scrollView.verticalScroller.value = scrollView.verticalScroller.highValue;
                UIManager.Instance.SetState(UIState.TitleScreen);

                yield break;
            }

            scrollView.verticalScroller.value = Mathf.Lerp(scrollView.verticalScroller.lowValue, scrollView.verticalScroller.highValue, timer / mainDuration);

            yield return null;
        }

        scrollView.verticalScroller.value = scrollView.verticalScroller.highValue;

        timer = 0;

        while ((timer += Time.unscaledDeltaTime) < bufferDuration)
        {
            if (Input.anyKey)
            {
                scrollView.verticalScroller.value = scrollView.verticalScroller.highValue;
                UIManager.Instance.SetState(UIState.TitleScreen);

                yield break;
            }

            yield return null;
        }

        UIManager.Instance.SetState(UIState.TitleScreen);
    }
}