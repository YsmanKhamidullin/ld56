using System;
using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

public class UpDownAnimation : MonoBehaviour
{
    [SerializeField]
    private float _addableY = 10f;

    [SerializeField]
    private float _time = 3f;

    [SerializeField]
    private Ease _ease = Ease.Linear;

    [SerializeField]
    private bool _isRandomize;

    [SerializeField]
    [HideInInspector]
    private bool _previewIsAdded;

    private void Start()
    {
        float startPosY = transform.localPosition.y;
        if (_isRandomize)
        {
            _time += Random.value;
        }
        transform.DOLocalMoveY(startPosY + _addableY, _time).SetEase(_ease).SetLoops(-1, LoopType.Yoyo);
    }

    [Button]
    private void Preview()
    {
        var curPos = transform.localPosition;
        curPos.y += _previewIsAdded ? -_addableY : _addableY;
        transform.localPosition = curPos;
        _previewIsAdded = !_previewIsAdded;
    }
}