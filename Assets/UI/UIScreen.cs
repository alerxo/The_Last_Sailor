using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class UIScreen : MonoBehaviour
{
    protected abstract List<UIState> ActiveStates { get; }
    public VisualElement Root { get; protected set; }
    [SerializeField] private StyleSheet[] styleSheets;

    private void OnEnable()
    {
        Root = GetComponent<UIDocument>().rootVisualElement;

        foreach (StyleSheet styleSheet in styleSheets)
        {
            Root.styleSheets.Add(styleSheet);
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
        Root.style.display = ActiveStates.Contains(_state) ? DisplayStyle.Flex : DisplayStyle.None;
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

    protected IEnumerator AnimateBorder(VisualElement _target, float _duration, float _start, float _end)
    {
        float timer = 0;

        while ((timer += Time.unscaledDeltaTime) < _duration)
        {
            SetBorder(_target, Mathf.Lerp(_start, _end, timer / _duration));

            yield return null;
        }

        SetBorder(_target, _end);
    }

    protected IEnumerator AnimateWidth(VisualElement _target, float _duration, float _start, float _end)
    {
        float timer = 0;

        while ((timer += Time.unscaledDeltaTime) < _duration)
        {
            SetWidth(_target, Mathf.Lerp(_start, _end, timer / _duration));

            yield return null;
        }

        SetWidth(_target, _end);
    }

    protected IEnumerator AnimateHeight(VisualElement _target, float _duration, float _start, float _end)
    {
        float timer = 0;

        while ((timer += Time.unscaledDeltaTime) < _duration)
        {
            SetHeight(_target, Mathf.Lerp(_start, _end, timer / _duration));

            yield return null;
        }

        SetHeight(_target, _end);
    }

    protected IEnumerator AnimateOpacity(List<VisualElement> _targets, float _duration, float _start, float _end)
    {
        float timer = 0;

        while ((timer += Time.unscaledDeltaTime) < _duration)
        {
            foreach (VisualElement target in _targets)
            {
                target.style.opacity = Mathf.Lerp(_start, _end, timer / _duration);
            }

            yield return null;
        }

        foreach (VisualElement target in _targets)
        {
            target.style.opacity = _end;
        }
    }
}