using UnityEngine;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour, IDropHandler
{
	private RectTransform _rt;
	private RectTransform _pdRectTransform;
	 
	public void OnDrop(PointerEventData eventData)
	{
		_rt = GetComponent<RectTransform>();
		_pdRectTransform = eventData.pointerDrag.GetComponent<RectTransform>();
		
		if (eventData.pointerDrag == null) return;
		if (name == "Grid")
		{
			_pdRectTransform.SetParent(_rt, false);
		}
		else
		{
			_pdRectTransform.SetParent(_rt, false);
			_pdRectTransform.anchoredPosition = new Vector2(.5f, .5f);
			_pdRectTransform.anchorMin = new Vector2(.5f, .5f);
			_pdRectTransform.anchorMax = new Vector2(.5f, .5f);
			_pdRectTransform.pivot = new Vector2(.5f, .5f);
			_pdRectTransform.localScale = _rt.localScale;
		}
	}
}

