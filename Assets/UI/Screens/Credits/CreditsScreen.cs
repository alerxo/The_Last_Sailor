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

        CreateLicenseCredit(scrollView, "Music", "Outrigger by Matthewmikemusic", "From Pixelbay", "Pixelbay Content License", " ", " ", 
            "Corsairs by StudioKolomna", "From Uppbeat:https://uppbeat.io/t/studiokolomna/corsairs" , "License code: GHAIY0KGPIACXFGU"
            );
        VisualElement smallBuffer = new();
        buffer.AddToClassList("credits-buffer");
        SetHeight(buffer, 150f);
        scrollView.Add(smallBuffer);

        CreateLicenseCredit(scrollView, "Sounds", "Flag flaps in the wind by Epic Stock media", "From Upbeats:https://uppbeat.io/sfx/flag-flaps-in-the-wind/8907/24172", " ", "",
            "Sizzle by CHEATman115", "Creative common 0 license", "", "",
            "Steam engine by SamsterBirdies", "Creative common 0 license", "", "",
            "Metal footsteps by Mypantsfelldown", "From Pixelbay", "Pixelbay Content License", "", "",
            "Rolling metal Barrel by Mateusz_Chenc", "Creative common 0 license", "", "",
            "big thud2 by Reitanna", "Creative common 0 license", "", "",
            "Thud4 by Yummy9987", "Creative common 0 license", "", "",
            "Distant Cannon Fire (simulated) by Stomachache", "Creative common 0 license", "",
            "Wood Creaking by Laft2k", "Creative common 0 license", "", "",
            "Wood creaking, close up_Zoom by Bini_trns", "Creative common 0 license", "", "",
            "Hitting Wood by Altfuture", "Creative common 0 license", "", "",
            "Seagulls Short by Lydmakeren", "Creative common 0 license", "", "",
            "Ship Bell two chimes by Sojan", "Creative common 0 license", "", "",
            "Jingle Lose__00 by LittleRobotSoundFactory", "Attribution 4.0", "", "",
            "Coin And Money Bag 1 by Floraphobic", "From Pixelbay", "Pixelbay Content License", "", "",
            "Newspaper Foley 4 by Floraphobic", "From Pixelbay", "Pixelbay Content License", "", "",
            "Pleasant Violin Notification 8 by Floraphobic", "From Pixelbay", "Pixelbay Content License", "", "",
            "Pleasant Violin Notification 10 by Floraphobic", "From Pixelbay", "Pixelbay Content License", "", "",
            "Metal Hit 91 by Floraphobic", "From Pixelbay", "Pixelbay Content License", "", "",
            "Metal Hit 94 by Floraphobic", "From Pixelbay", "Pixelbay Content License", "", "",
            "Metal Hit 95 by Floraphobic", "From Pixelbay", "Pixelbay Content License", "", "",
            "Rope Tighten Knot 4 by Floraphobic", "From Pixelbay", "Pixelbay Content License", "", "",
            "Rope Tighten Knot 5 by Floraphobic", "From Pixelbay", "Pixelbay Content License", "", "",
            "Rope Tighten Knot 6 by Floraphobic", "From Pixelbay", "Pixelbay Content License", "", "",
            "Rope Tighten Knot 9 by Floraphobic", "From Pixelbay", "Pixelbay Content License", "", "",
            "Exploding Building 1 by Floraphobic", "From Pixelbay", "Pixelbay Content License", "", "",
            "Fog horn 2 by Floraphobic", "From Pixelbay", "Pixelbay Content License", "", "",
            "camp fire 1 by LazyChillZone", "From Pixelbay", "Pixelbay Content License", "", "",
            "Explosion Sound Effect 1 by Cyberware-Ochestra", "From Pixelbay", "Pixelbay Content License", "", "",
            "Horror Scary Metal Screech by Imagine_impossible", "From Pixelbay", "Pixelbay Content License"
            );
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
        const float mainDuration = 90;

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