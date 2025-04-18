using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ItemPickupHandler : MonoBehaviour
{
    private Transform _player;
    private CharacterMovement _characterMovement;
    private GameObject _uiManager;
    private MenuHandler _menuHandler;
    private bool _isGamepad;

    [Header("References")]
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private UpdateButton updateButton1, updateButton2;
    [SerializeField] private DataHolder dataHolder;
    
    public List<PromptTrigger> promptTriggers;
    public PromptTrigger nearestPromptTrigger;
    
    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _characterMovement = _player.GetComponent<CharacterMovement>();
        _uiManager = GameObject.FindGameObjectWithTag("UIManager");
        _menuHandler = _uiManager.GetComponent<MenuHandler>();
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "Tutorial" || _characterMovement.uiOpen) return;
        nearestPromptTrigger = FindNearestTrigger();
        
        if (nearestPromptTrigger != null && promptTriggers.Count > 0)
        {
            _menuHandler.nearestTrigger = nearestPromptTrigger;
            TogglePrompt(nearestPromptTrigger.promptText, true, nearestPromptTrigger.button1, nearestPromptTrigger.button2);
        }
        else
        {
            TogglePrompt("", false, ControlsManager.ButtonType.None, ControlsManager.ButtonType.None);
        }
    }

    private PromptTrigger FindNearestTrigger()
    {
        PromptTrigger nearestTrigger = null;
        
        foreach (var trigger in promptTriggers)
        {
            if (!trigger.gameObject.activeSelf) continue;

            var dist = Vector3.Distance(transform.position, trigger.transform.position);

            if (trigger.doesOverrideOtherPrompts)
            {
                if (trigger.promptType != PromptTrigger.PromptType.Shop) return trigger;
                if (_menuHandler.shopGUI != null && !_menuHandler.shopGUI.activeSelf) continue;
                return trigger;
            }

            if (nearestTrigger == null)
            {
                nearestTrigger = trigger;
            }
            else if (Vector3.Distance(transform.position, nearestTrigger.transform.position) < dist)
            {
                nearestTrigger = trigger;
            }
            else if (Mathf.Approximately(Vector3.Distance(transform.position, nearestTrigger.transform.position), dist))
            {
                if (trigger.priority > nearestTrigger.priority)
                {
                    nearestTrigger = trigger;
                }
            }
        }
        
        return nearestTrigger;
    }
    
    public void TogglePrompt(string promptText, bool toggle, ControlsManager.ButtonType button, ControlsManager.ButtonType button2)
    {
        if (toggle)
        {
            if (button != ControlsManager.ButtonType.None)
            {
                rectTransform.anchoredPosition = new Vector3(0, 100, 0);
                text.text = promptText;
                updateButton1.button = button;
            }
            else
            {
                updateButton1.gameObject.SetActive(false);
            }

            if (button2 != ControlsManager.ButtonType.None)
            {
                updateButton2.button = button2;
                updateButton2.gameObject.SetActive(true);
            }
            else
            {
                updateButton2.gameObject.SetActive(false);
            }
        }
        else
        {
            rectTransform.anchoredPosition = new Vector3(0, -100, 0);
            text.text = "";
        }
    }
}
