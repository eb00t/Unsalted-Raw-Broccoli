using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class StatusTimer : MonoBehaviour
{
    public ConsumableEffect consumableEffect;
    private PlayerStatus _playerStatus;
    private Slider _slider;
    public float targetTime;
    public bool isTimerStarted;

    private void Start()
    {
        _slider = GetComponentInChildren<Slider>();
        _playerStatus = GameObject.FindGameObjectWithTag("UIManager").GetComponent<PlayerStatus>();
        _slider.maxValue = targetTime;
    }

    private void Update()
    {
        if (!isTimerStarted) return;
        
        targetTime -= Time.deltaTime;
        _slider.value = targetTime;
        
        if (targetTime <= 0.0f)
        {
            Debug.Log("Status removed as timer ended");
            _playerStatus.statuses.Remove(gameObject);
            Destroy(gameObject);
        }
    }
}
