using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// https://discussions.unity.com/t/scrollview-using-controller-arrowkeys/817360/15 adapted from here

[RequireComponent(typeof(ScrollRect))]
public class ScrollbarController : MonoBehaviour 
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform viewportRectTransform;
    [SerializeField] private RectTransform contentRectTransform;
    private RectTransform _selectedRectTransform;

    private void Update() 
    {
        var selected = EventSystem.current.currentSelectedGameObject;
        if (selected == null || !selected.transform.IsChildOf(contentRectTransform)) return;

        _selectedRectTransform = selected.GetComponent<RectTransform>();
        var viewportRect = viewportRectTransform.rect;

        var selectedRectViewport = _selectedRectTransform.rect
            .Transform(_selectedRectTransform)
            .InverseTransform(viewportRectTransform);

        var outsideOnTop = Mathf.Max(0, selectedRectViewport.yMax - viewportRect.yMax);
        var outsideOnBottom = Mathf.Max(0, viewportRect.yMin - selectedRectViewport.yMin);
        var delta = outsideOnTop > 0 ? outsideOnTop : -outsideOnBottom;

        if (delta == 0) return;

        var contentRectViewport = contentRectTransform.rect
            .Transform(contentRectTransform)
            .InverseTransform(viewportRectTransform);

        var overflow = contentRectViewport.height - viewportRect.height;
        var unitsToNormalized = 1 / overflow;

        scrollRect.verticalNormalizedPosition += delta * unitsToNormalized;
    }   
}

internal static class RectExtensions 
{
    public static Rect Transform(this Rect rect, Transform transform) 
    {
        return new Rect
        {
            min = transform.TransformPoint(rect.min),
            max = transform.TransformPoint(rect.max)
        };
    }
   
    public static Rect InverseTransform(this Rect rect, Transform transform) 
    {
        return new Rect
        {
            min = transform.InverseTransformPoint(rect.min),
            max = transform.InverseTransformPoint(rect.max)
        };
    }
}
