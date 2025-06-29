using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;

public class CurrencyManager : MonoBehaviour
{
    [SerializeField] private DataHolder dataHolder;
    [SerializeField] private TextMeshProUGUI currencyHeldText;
    [SerializeField] private float countDuration = 0.5f;
    [SerializeField] private GameObject newCurrencyObj;
    private Animator _animator;
    private TextMeshProUGUI _newCurrencyTxt;
    private Coroutine _currencyCounting;
    private Queue<int> _currencyQueue;
    private bool _isCounting;
    [SerializeField] private GameObject currency;
    public CanvasGroup canvasGroup;
    private MenuHandler _menuHandler;

    private void Start()
    {
        currencyHeldText.text = dataHolder.currencyHeld.ToString();
        _animator = newCurrencyObj.GetComponent<Animator>();
        _newCurrencyTxt = newCurrencyObj.GetComponent<TextMeshProUGUI>();
        _currencyQueue = new Queue<int>();
        canvasGroup = currency.GetComponent<CanvasGroup>();
        _menuHandler = GetComponent<MenuHandler>();
    }

    public void UpdateCurrency(int amount)
    {
        if (amount == 0) return;

        _currencyQueue.Enqueue(amount);

        if (!_isCounting)
        {
            StartCoroutine(Counting());
        }
    }
    
    private IEnumerator Counting()
    {
        _isCounting = true;
        if (_menuHandler.shopGUI != null && !_menuHandler.shopGUI.activeSelf || _menuHandler.shopGUI == null)
        {
            canvasGroup.DOFade(1, 0.5f);
        }

        while (_currencyQueue.Count > 0)
        {
            var amount = _currencyQueue.Dequeue();
            var oldValue = dataHolder.currencyHeld;
            dataHolder.currencyHeld = Mathf.Max(dataHolder.currencyHeld + amount, 0);
            var newValue = dataHolder.currencyHeld;
            var changeAmount = newValue - oldValue;

            if (changeAmount > 0)
            {
                dataHolder.playerCoilsCollected += changeAmount;
                dataHolder.totalCoilsCollected += changeAmount;
            }

            _newCurrencyTxt.text = (changeAmount > 0 ? "+ " : "- ") + Mathf.Abs(changeAmount);
            _animator.SetTrigger("currencyChanged");

            yield return StartCoroutine(UpdateOverTime(oldValue, newValue));
        }
        
        yield return new WaitForSeconds(1f);
        
        if (_menuHandler.shopGUI != null && !_menuHandler.shopGUI.activeSelf || _menuHandler.shopGUI == null)
        {
            canvasGroup.DOFade(0, 0.1f);
        }

        _isCounting = false;
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
