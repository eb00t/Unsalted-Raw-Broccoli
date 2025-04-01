using System.Collections;
using TMPro;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    [SerializeField] private DataHolder dataHolder;
    [SerializeField] private TextMeshProUGUI currencyHeldText;
    [SerializeField] private float countDuration = 0.5f;
    [SerializeField] private GameObject newCurrencyObj;
    private Animator _animator;
    private TextMeshProUGUI _newCurrencyTxt;
    private Coroutine _currencyCounting;

    private void Start()
    {
        currencyHeldText.text = dataHolder.currencyHeld.ToString();
        _animator = newCurrencyObj.GetComponent<Animator>();
        _newCurrencyTxt = newCurrencyObj.GetComponent<TextMeshProUGUI>();
    }

    public void UpdateCurrency(int amount)
    {
        var oldValue = dataHolder.currencyHeld;
        dataHolder.currencyHeld = Mathf.Max(dataHolder.currencyHeld + amount, 0);
        var newValue = dataHolder.currencyHeld;

        var changeAmount = newValue - oldValue;

        if (changeAmount != 0)
        {
            _newCurrencyTxt.text = (changeAmount > 0 ? "+ " : "- ") + Mathf.Abs(changeAmount);
            _animator.SetTrigger("currencyChanged");
        }

        if (_currencyCounting != null)
        {
            StopCoroutine(_currencyCounting);
        }

        _currencyCounting = StartCoroutine(UpdateOverTime(oldValue, newValue));
    }

    private IEnumerator UpdateOverTime(int startValue, int endValue)
    {
        var elapsed = 0f;

        while (elapsed < countDuration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / countDuration);
            var currentDisplay = Mathf.RoundToInt(Mathf.Lerp(startValue, endValue, t));
            currencyHeldText.text = currentDisplay.ToString();
            yield return null;
        }

        currencyHeldText.text = endValue.ToString();
    }
}
