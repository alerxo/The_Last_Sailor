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

        Generate();

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

    protected abstract void Generate();

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

    protected void SetMargin(VisualElement _target, float _top, float _bottom, float _left, float _right)
    {
        _target.style.marginTop = GetScaledValue(_top);
        _target.style.marginBottom = GetScaledValue(_bottom);
        _target.style.marginLeft = GetScaledValue(_left);
        _target.style.marginRight = GetScaledValue(_right);
    }

    protected float GetScaledValue(float _value)
    {
        return _value * UIManager.UIScale;
    }
}
