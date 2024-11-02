using UnityEngine;
using UnityEngine.UIElements;

public abstract class UIScreen : MonoBehaviour
{
    protected abstract UIState ActiveState { get; }
    protected UIDocument document;
    [SerializeField] private StyleSheet[] styleSheets;

    private void Awake()
    {
        document = GetComponent<UIDocument>();

        Generate();

        foreach(StyleSheet styleSheet in styleSheets)
        {
            document.rootVisualElement.styleSheets.Add(styleSheet);
        }
        
        UIManager.OnStateChanged += UIManager_OnStateChanged;
    }

    protected abstract void Generate();

    private void UIManager_OnStateChanged(UIState _state)
    {
        document.rootVisualElement.style.display = _state == ActiveState ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
