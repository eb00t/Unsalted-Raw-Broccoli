using System;
using UnityEngine;

public class CheckControls : MonoBehaviour
{
    [SerializeField] private DataHolder dataHolder;
    [SerializeField] private GameObject xboximg, psimg, keyimg;
    
    private GameObject _startMenu;
    private bool _isGamepad;
    private ControlsManager _controlsManager;

    private void Start()
    {
        _controlsManager = GameObject.FindGameObjectWithTag("UIManager").GetComponent<ControlsManager>();
    }

    private void Update()
    {
        if (!gameObject.activeSelf) return;
        UpdateImg();
    }

    private void UpdateImg()
    {
        _controlsManager.CheckControl();
        switch (dataHolder.currentControl)
        {
            case ControlsManager.ControlScheme.None:
            case ControlsManager.ControlScheme.Xbox:
                xboximg.SetActive(true);
                psimg.SetActive(false);
                keyimg.SetActive(false);
                break;
            case ControlsManager.ControlScheme.Playstation:
                xboximg.SetActive(false);
                psimg.SetActive(true);
                keyimg.SetActive(false);
                break;
            case ControlsManager.ControlScheme.Keyboard:
                xboximg.SetActive(false);
                psimg.SetActive(false);
                keyimg.SetActive(true);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
