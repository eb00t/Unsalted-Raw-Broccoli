using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusTimer : MonoBehaviour
{
    private PlayerStatus _playerStatus;
    private Slider _slider;
    public float targetTime;
    public bool isTimerStarted;
    private TextMeshProUGUI _text;
    private Consumable _consumable;

    private void Start()
    {
        _consumable = GetComponent<Consumable>();
        _slider = GetComponentInChildren<Slider>();
        _playerStatus = GameObject.FindGameObjectWithTag("UIManager").GetComponent<PlayerStatus>();
        _slider.maxValue = targetTime;
        _slider.value = _slider.maxValue;
    }

    private void Update()
    {
        if (!isTimerStarted) return;
        
        if (_consumable.consumableEffect == ConsumableEffect.Invincibility) return;
        
        targetTime -= Time.deltaTime;
        _slider.value = targetTime;
        
        if (targetTime <= 0.0f)
        {
            _playerStatus.statuses.Remove(gameObject);
            Destroy(gameObject);
        }
    }
}
