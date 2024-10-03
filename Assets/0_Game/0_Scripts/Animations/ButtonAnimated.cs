using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonAnimated : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField]
    private Transform _scaleRoot;

    [SerializeField]
    private Transform _animRoot;

    [SerializeField]
    private Image _colorImage;

    [SerializeField]
    private Color _selectedColor = Color.white;

    private Color _defaultColor;
    private bool _isHovering;

    private void Awake()
    {
        _defaultColor = _colorImage.color;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_isHovering)
        {
            return;
        }

        _colorImage.color = _selectedColor;
        Punch();
        _isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_isHovering)
        {
            ResetScale();
        }

        _colorImage.color = _defaultColor;
        _isHovering = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ResetScale();
        Punch();
    }

    public void Punch()
    {
        var newScale = Vector3.one * 1.1f;
        DOTween.Sequence()
            .Append(_animRoot.DOBlendablePunchRotation(_animRoot.forward * 1f, 0.5f, 6).SetEase(Ease.InOutSine))
            .Join(_scaleRoot.DOScale(newScale, 0.25f).SetEase(Ease.InSine)).Play();
    }

    private void ResetScale()
    {
        _scaleRoot.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutSine);
    }
}