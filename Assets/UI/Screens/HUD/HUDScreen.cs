using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class HUDScreen : UIScreen
{
    private const float BACKGROUND_WIDTH = 700f;
    private const float INTERACTION_BUTTON_SIZE = 100f;
    private const float INTERACTION_BUTTON_FONT_SIZE = 60;
    private const float INTERACTION_ANIMATION_DURATION = 0.1f;

    [SerializeField] private InputActionReference interactionAsset;

    protected override List<UIState> ActiveStates => new() { UIState.HUD };

    private Box admiralContainer;
    private Label admiralText;

    private Box interactionBackground;
    private Label interactionText;

    private Box exitInteractionBackground;

    private void Awake()
    {
        InteractionCollider.OnInteractableChanged += InteractionCollider_OnInteractableChanged;
        CombatManager.OnAdmiralInCombatChanged += CombatManager_OnAdmiralInCombatChanged;
        FirstPersonController.OnPlayerStateChanged += FirstPersonController_OnPlayerStateChanged;
    }

    private void OnDestroy()
    {
        InteractionCollider.OnInteractableChanged -= InteractionCollider_OnInteractableChanged;
        CombatManager.OnAdmiralInCombatChanged -= CombatManager_OnAdmiralInCombatChanged;
        FirstPersonController.OnPlayerStateChanged -= FirstPersonController_OnPlayerStateChanged;
    }

    private void InteractionCollider_OnInteractableChanged(IInteractable _interactable)
    {
        if (interactionBackground == null) return;

        if (_interactable == null)
        {
            StopCoroutine(ShowInteractionButton());
            StopCoroutine(HideInteractionButton());
            StartCoroutine(HideInteractionButton());
        }

        else
        {
            StopCoroutine(ShowInteractionButton());
            StopCoroutine(HideInteractionButton());
            StartCoroutine(ShowInteractionButton());
        }
    }

    private void CombatManager_OnAdmiralInCombatChanged(Admiral _admiral)
    {
        if (admiralContainer == null) return;

        if (_admiral != null)
        {
            admiralText.text = _admiral.Name;
            StopCoroutine(ShowAdmiralContainer());
            StartCoroutine(ShowAdmiralContainer());
        }
    }

    private void FirstPersonController_OnPlayerStateChanged(PlayerState _state)
    {
        switch (_state)
        {
            case PlayerState.FirstPerson:
                interactionBackground.style.display = DisplayStyle.Flex;
                exitInteractionBackground.style.display = DisplayStyle.None;
                break;

            case PlayerState.SteeringWheel:
            case PlayerState.Throttle:
            case PlayerState.Cannon:
                interactionBackground.style.display = DisplayStyle.None;
                exitInteractionBackground.style.display = DisplayStyle.Flex;
                break;
        }
    }

    public override void Generate()
    {
        VisualElement container = new();
        container.AddToClassList("hud-container");
        Root.Add(container);

        CreateAdmiral(container);
        CreateInteraction(container);
        CreateExitInteraction(container);

        HideAdmiral();
        HideInteraction();
    }

    private void CreateAdmiral(VisualElement _parent)
    {
        admiralContainer = new();
        admiralContainer.AddToClassList("hud-admiral-container");
        SetMargin(admiralContainer, 50, 0, 0, 0);
        SetBorderRadius(admiralContainer, 10);
        _parent.Add(admiralContainer);

        admiralText = new("Admiral");
        admiralText.AddToClassList("hud-admiral-text");
        SetPadding(admiralText, 10);
        SetFontSize(admiralText, 50);
        admiralContainer.Add(admiralText);
    }

    private IEnumerator ShowAdmiralContainer()
    {
        const float BORDER_WIDTH = 5f;
        const float BORDER_DURATION = 0.1f;
        const float BACKGROUND_DURATION = 0.5f;

        HideAdmiral();

        yield return AnimateBorderWidth(admiralContainer, BORDER_DURATION, 0, 5f);
        yield return AnimateWidth(admiralContainer, BACKGROUND_DURATION, 0, BACKGROUND_WIDTH);

        yield return new WaitForSeconds(10f);

        yield return AnimateWidth(admiralContainer, BACKGROUND_DURATION, BACKGROUND_WIDTH, 0);
        yield return AnimateBorderWidth(admiralContainer, BORDER_DURATION, BORDER_WIDTH, 0);
    }

    private void HideAdmiral()
    {
        SetWidth(admiralContainer, 0);
        SetBorderWidth(admiralContainer, 0);
    }

    private void CreateInteraction(VisualElement _parent)
    {
        VisualElement interactionContainer = new();
        interactionContainer.AddToClassList("hud-interaction-container");
        SetMargin(interactionContainer, 0, 100, 0, 0);
        SetSize(interactionContainer, INTERACTION_BUTTON_SIZE, INTERACTION_BUTTON_SIZE);
        _parent.Add(interactionContainer);

        interactionBackground = new();
        interactionBackground.AddToClassList("hud-interaction-background");
        SetSize(interactionBackground, 0, 0);
        SetBorderWidth(interactionBackground, 0);
        interactionContainer.Add(interactionBackground);

        interactionText = new(InputControlPath.ToHumanReadableString(interactionAsset.action.bindings[0].effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice));
        interactionText.AddToClassList("hud-interaction-text");
        SetFontSize(interactionText, 0);
        interactionBackground.Add(interactionText);
    }

    private IEnumerator ShowInteractionButton()
    {
        HideInteraction();

        float duration = 0;

        while ((duration += Time.deltaTime) < INTERACTION_ANIMATION_DURATION)
        {
            float percentage = duration / INTERACTION_ANIMATION_DURATION;
            float size = Mathf.Lerp(0, INTERACTION_BUTTON_SIZE, percentage);
            SetSize(interactionBackground, size, size);
            SetFontSize(interactionText, Mathf.Lerp(0, INTERACTION_BUTTON_FONT_SIZE, percentage));

            yield return null;
        }

        ShowInteraction();
    }

    private IEnumerator HideInteractionButton()
    {
        ShowInteraction();

        float duration = 0;

        while ((duration += Time.deltaTime) < INTERACTION_ANIMATION_DURATION)
        {
            float percentage = duration / INTERACTION_ANIMATION_DURATION;
            float size = Mathf.Lerp(INTERACTION_BUTTON_SIZE, 0, percentage);
            SetSize(interactionBackground, size, size);
            SetFontSize(interactionText, Mathf.Lerp(INTERACTION_BUTTON_FONT_SIZE, 0, percentage));

            yield return null;
        }

        HideInteraction();
    }

    private void ShowInteraction()
    {
        SetSize(interactionBackground, INTERACTION_BUTTON_SIZE, INTERACTION_BUTTON_SIZE);
        SetFontSize(interactionText, INTERACTION_BUTTON_FONT_SIZE);
    }

    private void HideInteraction()
    {
        SetSize(interactionBackground, 0, 0);
        SetFontSize(interactionText, 0);
    }

    private void CreateExitInteraction(VisualElement _parent)
    {
        exitInteractionBackground = new();
        exitInteractionBackground.AddToClassList("hud-exit-interaction-background");
        SetMargin(exitInteractionBackground, 0, 100, 0, 0);
        SetPadding(exitInteractionBackground, 10);
        SetBorderWidthRadius(exitInteractionBackground, 0, 10);
        _parent.Add(exitInteractionBackground);

        Label text = new("Press E to exit");
        text.AddToClassList("hud-exit-interaction-text");
        SetFontSize(text, 35);
        exitInteractionBackground.Add(text);
    }
}