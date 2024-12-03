using UnityEngine;
using UnityEngine.UIElements;

public abstract class UIScreen : MonoBehaviour
{
    protected abstract UIState ActiveState { get; }
    protected VisualElement root;
    [SerializeField] private StyleSheet[] styleSheets;

    private void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        foreach (StyleSheet styleSheet in styleSheets)
        {
            root.styleSheets.Add(styleSheet);
        }

        UIManager.OnStateChanged += UIManager_OnStateChanged;
    }

    private void OnDisable()
    {
        UIManager.OnStateChanged -= UIManager_OnStateChanged;
    }

    public abstract void Generate();

    private void UIManager_OnStateChanged(UIState _state)
    {
        root.style.display = _state == ActiveState ? DisplayStyle.Flex : DisplayStyle.None;
    }

    protected void SetSize(VisualElement _target, float _width, float _height)
    {
        SetWidth(_target, _width);
        SetHeight(_target, _height);
    }

    protected void SetWidth(VisualElement _target, float _value)
    {
        _target.style.width = GetScaledValue(_value);
    }

    protected void SetHeight(VisualElement _target, float _value)
    {
        _target.style.height = GetScaledValue(_value);
    }

    protected void SetFontSize(VisualElement _target, float _value)
    {
        _target.style.fontSize = GetScaledValue(_value);
    }

    protected void SetBorder(VisualElement _target, float _value)
    {
        _target.style.borderTopWidth = GetScaledValue(_value);
        _target.style.borderBottomWidth = GetScaledValue(_value);
        _target.style.borderLeftWidth = GetScaledValue(_value);
        _target.style.borderRightWidth = GetScaledValue(_value);
    }

    public static void SetBorder(VisualElement _target, Color _color)
    {
        _target.style.borderBottomColor = _color;
        _target.style.borderTopColor = _color;
        _target.style.borderLeftColor = _color;
        _target.style.borderRightColor = _color;
    }

    protected float GetScaledValue(float _value)
    {
        return _value * UIManager.UIScale;
    }
}