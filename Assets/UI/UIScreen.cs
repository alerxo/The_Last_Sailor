using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

    protected void SetMargin(VisualElement _target, float _value)
    {
        SetMargin(_target, _value, _value, _value, _value);
    }

    protected void SetMargin(VisualElement _target, float _top, float _bottom, float _left, float _right)
    {
        _target.style.marginTop = GetScaledValue(_top);
        _target.style.marginBottom = GetScaledValue(_bottom);
        _target.style.marginLeft = GetScaledValue(_left);
        _target.style.marginRight = GetScaledValue(_right);
    }

    protected void SetPadding(VisualElement _target, float _value)
    {
        SetPadding(_target, _value, _value, _value, _value);
    }

    protected void SetPadding(VisualElement _target, float _top, float _bottom, float _left, float _right)
    {
        _target.style.paddingTop = GetScaledValue(_top);
        _target.style.paddingBottom = GetScaledValue(_bottom);
        _target.style.paddingLeft = GetScaledValue(_left);
        _target.style.paddingRight = GetScaledValue(_right);
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

    protected void SetBorderWidthRadius(VisualElement _target, float _width, float _radius)
    {
        SetBorderWidth(_target, _width);
        SetBorderRadius(_target, _radius);
    }

    protected void SetBorderWidth(VisualElement _target, float _value)
    {
        _target.style.borderTopWidth = GetScaledValue(_value);
        _target.style.borderBottomWidth = GetScaledValue(_value);
        _target.style.borderLeftWidth = GetScaledValue(_value);
        _target.style.borderRightWidth = GetScaledValue(_value);
    }

    protected void SetBorderRadius(VisualElement _target, float _value)
    {
        _target.style.borderTopLeftRadius = GetScaledValue(_value);
        _target.style.borderTopRightRadius = GetScaledValue(_value);
        _target.style.borderBottomLeftRadius = GetScaledValue(_value);
        _target.style.borderBottomRightRadius = GetScaledValue(_value);
    }

    public static void SetBorderColor(VisualElement _target, Color _color)
    {
        _target.style.borderBottomColor = _color;
        _target.style.borderTopColor = _color;
        _target.style.borderLeftColor = _color;
        _target.style.borderRightColor = _color;
    }

    protected void SetFontSize(VisualElement _target, float _value)
    {
        _target.style.fontSize = GetScaledValue(_value);
    }

    protected float GetScaledValue(float _value)
    {
        return _value * UIManager.UIScale;
    }

    protected IEnumerator AnimateBorderWidth(VisualElement _target, float _duration, float _start, float _end)
    {
        float timer = 0;

        while ((timer += Time.unscaledDeltaTime) < _duration)
        {
            SetBorderWidth(_target, Mathf.Lerp(_start, _end, timer / _duration));

            yield return null;
        }

        SetBorderWidth(_target, _end);
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

    protected IEnumerator AnimateNumber(Label _target, float _duration, float _start, float _end)
    {
        float timer = 0;

        while ((timer += Time.unscaledDeltaTime) < _duration)
        {
            _target.text = FormatAnimatedNumber(Mathf.Lerp(_start, _end, timer / _duration));
            yield return null;
        }

        _target.text = FormatAnimatedNumber(_end);
    }

    private string FormatAnimatedNumber(float _value)
    {
        return string.Format("{0:F1}", _value).Replace(".0", "");
    }

    protected IEnumerator AnimateScrollviewScrollUp(List<ScrollView> _targets, float _duration)
    {
        float timer = 0;

        while ((timer += Time.unscaledDeltaTime) < _duration)
        {
            foreach (ScrollView scrollView in _targets)
            {
                scrollView.verticalScroller.value = Mathf.Lerp(scrollView.verticalScroller.highValue, scrollView.verticalScroller.lowValue, timer / _duration);
            }

            yield return null;
        }

        foreach (ScrollView scrollView in _targets)
        {
            scrollView.verticalScroller.value = scrollView.verticalScroller.lowValue;
        }
    }

    protected IEnumerator AnimateScrollviewScrollDown(List<ScrollView> _targets, List<VisualElement> _player, List<VisualElement> _enemy, float _durationPerItem)
    {
        float totalTimer = 0;
        float totalDuration = Mathf.Max(_enemy.Count, _player.Count) * _durationPerItem;

        for (int i = 0; i < Mathf.Max(_player.Count, _enemy.Count); i++)
        {
            float timer = 0;

            if (_player.Count > i)
            {
                _player[i].SetEnabled(true);
            }

            if (_enemy.Count > i)
            {
                _enemy[i].SetEnabled(true);
            }

            while ((timer += Time.unscaledDeltaTime) < _durationPerItem)
            {
                totalTimer += Time.unscaledDeltaTime;

                foreach (ScrollView scrollView in _targets)
                {
                    scrollView.verticalScroller.value = Mathf.Lerp(scrollView.verticalScroller.lowValue, scrollView.verticalScroller.highValue, totalTimer / totalDuration);
                }

                yield return null;
            }
        }

        foreach (ScrollView scrollView in _targets)
        {
            scrollView.verticalScroller.value = scrollView.verticalScroller.highValue;
        }
    }

    protected IEnumerator AnimateOpacity(VisualElement _target, float _duration, float _start, float _end)
    {
        float timer = 0;

        while ((timer += Time.unscaledDeltaTime) < _duration)
        {
            _target.style.opacity = Mathf.Lerp(_start, _end, timer / _duration);

            yield return null;
        }

        _target.style.opacity = _end;
    }
}