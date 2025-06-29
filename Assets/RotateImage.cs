using UnityEngine;
using DG.Tweening;

public class RotateImage : MonoBehaviour
{ 
    private void Start()
    {
        transform.DORotate(new Vector3(0, 0, -360), 6f, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart);
    }
}
