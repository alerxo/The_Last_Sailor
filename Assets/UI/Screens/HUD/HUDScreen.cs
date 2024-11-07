using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class HUDScreen : UIScreen
{
    [SerializeField] private InputActionReference interactionAsset;

    protected override UIState ActiveState => UIState.HUD;

    private Box interactionContainer;

    private void Awake()
    {
        InteractionCollider.OnInteractableChanged += InteractionCollider_OnInteractableChanged;
    }

    private void OnDestroy()
    {
        InteractionCollider.OnInteractableChanged -= InteractionCollider_OnInteractableChanged;
    }

    private void InteractionCollider_OnInteractableChanged(IInteractable _interactable)
    {
        if (interactionContainer == null) return;

        interactionContainer.style.display = _interactable != null ? DisplayStyle.Flex : DisplayStyle.None;
    }

    protected override void Generate()
    {
        VisualElement container = new();
        container.AddToClassList("hud-container");
        root.Add(container);

        interactionContainer = new();
        interactionContainer.AddToClassList("hud-interaction-container");
        SetSize(interactionContainer, 128, 128);
        interactionContainer.style.display = DisplayStyle.None;
        container.Add(interactionContainer);

        Label interactionText = new(InputControlPath.ToHumanReadableString(interactionAsset.action.bindings[0].effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice));
        interactionText.AddToClassList("hud-interaction-text");
        SetFontSize(interactionText, 80);
        interactionContainer.Add(interactionText);
    }
}