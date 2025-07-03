using System;
using UnityEngine;
using DG.Tweening;

public class RotateImage : MonoBehaviour
{
    private Tween _tween;
    private void Start()
    {
        _tween = transform.DORotate(new Vector3(0, 0, -360), 6f, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart);
    }

    private void OnDestroy()
    {
        _tween?.Kill();
    }
    
    private void OnDisable()
    {
        _tween?.Kill();
    }
}
