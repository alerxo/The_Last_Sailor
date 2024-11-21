using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class HUDScreen : UIScreen
{
    private const float ADMIRAL_WIDTH = 700f;
    private const float ADMIRAL_BORDER_WIDTH = 5f;
    private const float ADMIRAL_DURATION = 10f;
    private const float ADMIRAL_BORDER_DURATION = 0.1f;
    private const float ADMIRAL_BACKGROUND_DURATION = 0.5f;

    private const float INTERACTION_BUTTON_SIZE = 128f;
    private const float INTERACTION_BUTTON_FONT_SIZE = 80;
    private const float INTERACTION_BUTTON_BORDER_WIDTH = 5;
    private const float INTERACTION_ANIMATION_DURATION = 0.1f;

    [SerializeField] private InputActionReference interactionAsset;

    protected override UIState ActiveState => UIState.HUD;

    private Box admiralContainer;
    private Label admiralText;

    private Box interactionBackground;
    private Label interactionText;

    private void Awake()
    {
        InteractionCollider.OnInteractableChanged += InteractionCollider_OnInteractableChanged;
        CombatManager.OnAdmiralInCombatChanged += CombatManager_OnAdmiralInCombatChanged;
    }

    private void OnDestroy()
    {
        InteractionCollider.OnInteractableChanged -= InteractionCollider_OnInteractableChanged;
        CombatManager.OnAdmiralInCombatChanged -= CombatManager_OnAdmiralInCombatChanged;
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
            StopCoroutine(ShowAdmiral());
            StartCoroutine(ShowAdmiral());
        }
    }

    public override void Generate()
    {
        VisualElement container = new();
        container.AddToClassList("hud-container");
        root.Add(container);

        CreateAdmiral(container);
        CreateInteraction(container);
    }

    private void CreateAdmiral(VisualElement container)
    {
        admiralContainer = new();
        admiralContainer.AddToClassList("hud-admiral-container");
        SetWidth(admiralContainer, 0);
        SetBorder(admiralContainer, 0);
        container.Add(admiralContainer);

        Box admiralBackground = new();
        admiralBackground.AddToClassList("hud-admiral-background");
        SetWidth(admiralBackground, ADMIRAL_WIDTH);
        admiralContainer.Add(admiralBackground);

        admiralText = new("Admiral");
        admiralText.AddToClassList("hud-interaction-text");
        SetFontSize(admiralText, 50);
        admiralBackground.Add(admiralText);
    }

    private IEnumerator ShowAdmiral()
    {
        SetWidth(admiralContainer, 0);
        SetBorder(admiralContainer, 0);

        // Show Border

        float duration = 0;

        while ((duration += Time.deltaTime) < ADMIRAL_BORDER_DURATION)
        {
            float percentage = duration / ADMIRAL_BORDER_DURATION;
            SetBorder(admiralContainer, Mathf.Lerp(0, ADMIRAL_BORDER_WIDTH, percentage));

            yield return null;
        }

        SetBorder(admiralContainer, ADMIRAL_BORDER_WIDTH);

        // Show Container

        duration = 0;

        while ((duration += Time.deltaTime) < ADMIRAL_BACKGROUND_DURATION)
        {
            float percentage = duration / ADMIRAL_BACKGROUND_DURATION;
            SetWidth(admiralContainer, Mathf.Lerp(0, ADMIRAL_WIDTH, percentage));

            yield return null;
        }

        SetWidth(admiralContainer, ADMIRAL_WIDTH);

        // Stay

        yield return new WaitForSeconds(ADMIRAL_DURATION);

        // Hide Container

        duration = 0;

        while ((duration += Time.deltaTime) < ADMIRAL_BACKGROUND_DURATION)
        {
            float percentage = duration / ADMIRAL_BACKGROUND_DURATION;
            SetWidth(admiralContainer, Mathf.Lerp(ADMIRAL_WIDTH, 0, percentage));

            yield return null;
        }

        SetWidth(admiralContainer, 0);

        // Hide Border

        duration = 0;

        while ((duration += Time.deltaTime) < ADMIRAL_BORDER_DURATION)
        {
            float percentage = duration / ADMIRAL_BORDER_DURATION;
            SetBorder(admiralContainer, Mathf.Lerp(ADMIRAL_BORDER_WIDTH, 0, percentage));

            yield return null;
        }

        SetBorder(admiralContainer, 0);
    }

    private void CreateInteraction(VisualElement container)
    {
        VisualElement interactionContainer = new();
        interactionContainer.AddToClassList("hud-interaction-container");
        SetSize(interactionContainer, INTERACTION_BUTTON_SIZE, INTERACTION_BUTTON_SIZE);
        container.Add(interactionContainer);

        interactionBackground = new();
        interactionBackground.AddToClassList("hud-interaction-background");
        SetSize(interactionBackground, 0, 0);
        SetBorder(interactionBackground, 0);
        interactionContainer.Add(interactionBackground);

        interactionText = new(InputControlPath.ToHumanReadableString(interactionAsset.action.bindings[0].effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice));
        interactionText.AddToClassList("hud-interaction-text");
        SetFontSize(interactionText, 0);
        interactionBackground.Add(interactionText);
    }

    private IEnumerator ShowInteractionButton()
    {
        SetSize(interactionBackground, 0, 0);
        SetFontSize(interactionText, 0);
        SetBorder(interactionBackground, 0);

        float duration = 0;

        while ((duration += Time.deltaTime) < INTERACTION_ANIMATION_DURATION)
        {
            float percentage = duration / INTERACTION_ANIMATION_DURATION;
            float size = Mathf.Lerp(0, INTERACTION_BUTTON_SIZE, percentage);
            SetSize(interactionBackground, size, size);
            SetFontSize(interactionText, Mathf.Lerp(0, INTERACTION_BUTTON_FONT_SIZE, percentage));
            SetBorder(interactionBackground, Mathf.Lerp(0, INTERACTION_BUTTON_BORDER_WIDTH, percentage));

            yield return null;
        }

        SetSize(interactionBackground, INTERACTION_BUTTON_SIZE, INTERACTION_BUTTON_SIZE);
        SetFontSize(interactionText, INTERACTION_BUTTON_FONT_SIZE);
        SetBorder(interactionBackground, INTERACTION_BUTTON_BORDER_WIDTH);
    }

    private IEnumerator HideInteractionButton()
    {
        SetSize(interactionBackground, INTERACTION_BUTTON_SIZE, INTERACTION_BUTTON_SIZE);
        SetFontSize(interactionText, INTERACTION_BUTTON_FONT_SIZE);
        SetBorder(interactionBackground, INTERACTION_BUTTON_BORDER_WIDTH);

        float duration = 0;

        while ((duration += Time.deltaTime) < INTERACTION_ANIMATION_DURATION)
        {
            float percentage = duration / INTERACTION_ANIMATION_DURATION;
            float size = Mathf.Lerp(INTERACTION_BUTTON_SIZE, 0, percentage);
            SetSize(interactionBackground, size, size);
            SetFontSize(interactionText, Mathf.Lerp(INTERACTION_BUTTON_FONT_SIZE, 0, percentage));
            SetBorder(interactionBackground, Mathf.Lerp(INTERACTION_BUTTON_BORDER_WIDTH, 0, percentage));

            yield return null;
        }

        SetSize(interactionBackground, 0, 0);
        SetFontSize(interactionText, 0);
        SetBorder(interactionBackground, 0);
    }
}