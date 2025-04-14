using TMPro;
using UnityEngine;
using System.Collections;

public class TitleType : MonoBehaviour
{
    private TextMeshProUGUI _text;
    [SerializeField] private float typeDelay;
    [SerializeField] private float blinkRate;

    private string _title = "  Blank\nConstruct";
    private string _visibleText = "";
    private bool _showCursor = true;

    private void Start()
    {
        _text = GetComponent<TextMeshProUGUI>();
        StartCoroutine(TypeText());
        InvokeRepeating(nameof(BlinkCursor), blinkRate, blinkRate);
    }

    private IEnumerator TypeText()
    {
        for (var i = 0; i <= _title.Length; i++)
        {
            _visibleText = _title[..i];
            UpdateDisplay();
            yield return new WaitForSeconds(typeDelay);
        }
    }

    private void BlinkCursor()
    {
        _showCursor = !_showCursor;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        var cursor = _showCursor ? "_" : " ";
        _text.text = _visibleText + cursor;
    }
}
