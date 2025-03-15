using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpdateButton : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private DataHolder dataHolder;
    private GameObject _uiManager;
    private ControlsManager _controlsManager;
    public ControlsManager.ButtonType button;

    private void Start()
    {
        _uiManager = GameObject.FindGameObjectWithTag("UIManager");
        _controlsManager = _uiManager.GetComponent<ControlsManager>();
    }

    private void Update()
    {
        UpdateControl();
    }

    private void UpdateControl()
    {
        switch (dataHolder.currentControl)
        {
            case ControlsManager.ControlScheme.Xbox:
                text.gameObject.SetActive(false);
                image.gameObject.SetActive(true);
                image.sprite = _controlsManager.XboxSprites.ContainsKey(button) ? _controlsManager.XboxSprites[button] : null;
                break;
            case ControlsManager.ControlScheme.Playstation:
                text.gameObject.SetActive(false);
                image.gameObject.SetActive(true);
                image.sprite = _controlsManager.PlaystationSprites.ContainsKey(button) ? _controlsManager.PlaystationSprites[button] : null;
                break;
            case ControlsManager.ControlScheme.Keyboard:
                image.gameObject.SetActive(false);
                text.gameObject.SetActive(true);
                
                text.text = _controlsManager.KeyboardStrings.ContainsKey(button) ? _controlsManager.KeyboardStrings[button] : "???";
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
