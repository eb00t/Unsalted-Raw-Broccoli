using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TimedDestroy : MonoBehaviour
{
	[SerializeField] private Transform img, text;
	public float notificationDuration;
    
    private void Awake()
    {
	    transform.localScale = new Vector3(0, 0.1f, 1);;
	    img.localScale = new Vector3(1, 0, 1);
	    text.localScale = new Vector3(1, 0, 1);
	    
	    var notifSeq = DOTween.Sequence().SetUpdate(true);
	    notifSeq.Append(transform.DOScale(new Vector3(1, 0.15f, 1), 0.1f).SetEase(Ease.OutBack));
	    notifSeq.Append(transform.DOScale(Vector3.one, 0.15f).SetEase(Ease.OutBack));

	    notifSeq.OnComplete(() =>
	    {
		    img.DOScaleY(1f, 0.1f).SetUpdate(true);
		    text.DOScaleY(1f, 0.1f).SetUpdate(true);
		    StartCoroutine(WaitToDisappear());
	    });
    }

    private IEnumerator WaitToDisappear()
    {
	    yield return new WaitForSecondsRealtime(notificationDuration);
	    
	    var notifCloseSeq = DOTween.Sequence().SetUpdate(true);
			
	    notifCloseSeq.Append(img.DOScaleY(0f, 0.1f));
	    notifCloseSeq.Append(text.DOScaleY(0f, 0.1f));
	    notifCloseSeq.Append(transform.DOScale(new Vector3(1, 0, 1), 0.1f).SetEase(Ease.InBack));

	    notifCloseSeq.OnComplete(() =>
	    {
		    notifCloseSeq.Kill();
		    Destroy(gameObject);
	    });
    }

    public void DestroyThis()
	{
		Destroy(gameObject);
	}
}
