using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomSlider : MonoBehaviour
{
    public event Action<float> OnValueChanged;

    [SerializeField]
    private TextMeshProUGUI _valueLabel;

    [SerializeField]
    private Slider _slider;

    [SerializeField]
    private int _roundDigits = 1;

    [SerializeField]
    private int _multiplyBy = 100;

    private string _defaultFormat;

    private void Awake()
    {
        _defaultFormat = _valueLabel.text;
        _slider.onValueChanged.AddListener(CallOnValueChanged);
    }

    private void CallOnValueChanged(float normalized)
    {
        OnValueChanged?.Invoke(normalized);
        UpdateValueLabel(normalized);
    }

    private void UpdateValueLabel(float normalized)
    {
        normalized *= _multiplyBy;
        var rounded = Math.Round(normalized, _roundDigits);
        _valueLabel.text = string.Format(_defaultFormat, rounded);
    }

    public void SetValueWithoutNotify(float musicValue)
    {
        _slider.SetValueWithoutNotify(musicValue);
        UpdateValueLabel(musicValue);
    }
}