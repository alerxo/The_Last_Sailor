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
        scrollView.AddToClassList("credits-item-container");
        scrollView.RegisterCallback<WheelEvent>((e) => e.StopPropagation(), TrickleDown.TrickleDown);
        scrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
        scrollView.horizontalScroller.RemoveFromHierarchy();
        container.Add(scrollView);

        CreateBuffer();

        CreateCredit(scrollView, "Alexander Robinson", "www.alexanderrobinson.se", "Programmer", "UI & UX Designer");
        CreateCredit(scrollView, "Viktor Eriksson", "ArtStation: viktor_eriksson", "www.artstation.com/viktor_eriksson", "3D-Artist", "Most VFX", "Minor Systems");
        CreateCredit(scrollView, "Jacob Nordenvall", "jacobnordenvall.wixsite.com/jacobnordenvall", "Sound Designer", "2D-Artist");

        VisualElement buffer = new();
        buffer.AddToClassList("credits-buffer");
        SetHeight(buffer, 300f);
        scrollView.Add(buffer);

        CreateLicenseCredit(scrollView, "Music", "Outrigger by Matthewmikemusic from Pixelbay Content License");

        CreateBuffer();
    }

    private void CreateBuffer()
    {
        VisualElement buffer = new();
        buffer.AddToClassList("credits-buffer");
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

        scrollView.parent.SetEnabled(false);
    }

    private void CreateLicenseCredit(VisualElement _parent, string _header, params string[] _content)
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
            SetWidth(label, 800);
            container.Add(label);
        }

        scrollView.parent.SetEnabled(false);
    }

    private IEnumerator CreditScroll()
    {
        float timer = 0;
        const float bufferDuration = 1;
        const float mainDuration = 30;

        scrollView.parent.SetEnabled(true);

        scrollView.verticalScroller.value = scrollView.verticalScroller.lowValue;

        while ((timer += Time.unscaledDeltaTime) < bufferDuration)
        {
            if (Input.anyKey)
            {
                scrollView.verticalScroller.value = scrollView.verticalScroller.highValue;
                UIManager.Instance.SetState(UIState.TitleScreen);
                scrollView.parent.SetEnabled(false);

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
                scrollView.parent.SetEnabled(false);

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
                scrollView.parent.SetEnabled(false);

                yield break;
            }

            yield return null;
        }

        UIManager.Instance.SetState(UIState.TitleScreen);
        scrollView.parent.SetEnabled(false);
    }
}