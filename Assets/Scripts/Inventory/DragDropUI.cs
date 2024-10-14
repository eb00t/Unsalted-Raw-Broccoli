using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragDropUI : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
{
    private RectTransform _rectTransform;
	private Canvas _canvas;
	private Transform _overLayer;
    private CanvasGroup _canvasGroup;
    //public Weapons weapons;
    
    private void Start()
    {
	    _rectTransform = GetComponent<RectTransform>();
	    _canvas = GetComponentInParent<RectTransform>().gameObject.GetComponentInParent<Canvas>();
	    _overLayer = GameObject.Find("overlayer").transform;
	    _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
	    transform.SetParent(_overLayer);
	    _canvasGroup.blocksRaycasts = false;
	    _canvasGroup.alpha = .6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
	    _rectTransform.anchoredPosition += eventData.delta / _canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
	    _canvasGroup.blocksRaycasts = true;
	    _canvasGroup.alpha = 1f;
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
	    Debug.Log("Pointer Down");
    }

    public void OnDrop(PointerEventData eventData)
    {
	    Debug.Log("OnDrop");
    }

    //public List<recipe> recipes;
    //private GameObject _selected;
}