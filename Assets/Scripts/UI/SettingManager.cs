using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class SettingManager : MonoBehaviour
{
    private InventoryStore _inventoryStore;
    [SerializeField] private DataHolder dataHolder;
    [SerializeField] private Slider masterSlider, musicSlider, sfxSlider, screenShakeSlider;
    [SerializeField] private TMP_Dropdown controlSchemeDropdown;
    [SerializeField] private Toggle autoEquipToggle, autoLockOnToggle, autoSwitchToggle;
    [Range(0, 1)]
    public float screenShakeMultiplier = 1;
    private float _currentSlider;
    private float _currentSliderValue;
    private AudioManager _audioManager;
    private GameObject _lastSelected;
    
    private void Start()
    {
        _audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
        screenShakeMultiplier = dataHolder.screenShakeMultiplier;
        LoadSettings();
    }

    private void Update()
    {
        if (EventSystem.current.currentSelectedGameObject != _lastSelected)
        {
            if (dataHolder.currentControl == ControlsManager.ControlScheme.Keyboard) return;
            ButtonHandler.Instance.PlayNavigateSound();
            _lastSelected = EventSystem.current.currentSelectedGameObject;
        }
    }

    public void ToggleAutoEquip()
    {
        dataHolder.isAutoEquipEnabled = !dataHolder.isAutoEquipEnabled;
        ButtonHandler.Instance.PlayConfirmSound();
    }

    public void ToggleAutoLockOn()
    {
        dataHolder.isAutoLockOnEnabled = !dataHolder.isAutoLockOnEnabled;
        ButtonHandler.Instance.PlayConfirmSound();
    }

    public void ToggleAutoSwitchLockTarget()
    {
        dataHolder.isAutoSwitchEnabled = !dataHolder.isAutoSwitchEnabled;
        ButtonHandler.Instance.PlayConfirmSound();
    }

    private void LoadSettings()
    {
        // sliders
        _audioManager.masterVolume = dataHolder.masterVolume;
        _audioManager.ambienceVolume = dataHolder.musicVolume;
        _audioManager.musicVolume = dataHolder.musicVolume;
        _audioManager.sfxVolume = dataHolder.sfxVolume;
        _audioManager.uiSfxVolume = dataHolder.sfxVolume;
        masterSlider.value = dataHolder.masterVolume;
        musicSlider.value = dataHolder.musicVolume;
        sfxSlider.value = dataHolder.sfxVolume;
        
        screenShakeSlider.value = dataHolder.screenShakeMultiplier;
        
        // toggles
        autoEquipToggle.isOn = dataHolder.isAutoEquipEnabled;
        autoSwitchToggle.isOn = dataHolder.isAutoSwitchEnabled;
        autoLockOnToggle.isOn = dataHolder.isAutoLockOnEnabled;
        
        // dropdowns
        if (dataHolder.forceControlScheme)
        {
            switch (dataHolder.currentControl)
            {
                case ControlsManager.ControlScheme.Xbox:
                    controlSchemeDropdown.value = 1;
                    break;
                case ControlsManager.ControlScheme.Playstation:
                    controlSchemeDropdown.value = 2;
                    break;
                case ControlsManager.ControlScheme.Keyboard:
                    controlSchemeDropdown.value = 3;
                    break;
            }
        }
    }

    public void UpdateScreenShake()
    {
        screenShakeMultiplier = screenShakeSlider.value;
        dataHolder.screenShakeMultiplier = screenShakeSlider.value;
    }

    public void UpdateVolume(int volumeType)
    {
        // gets current selected gameobject (slider handle)
        
        var selected = EventSystem.current.currentSelectedGameObject;

        if (selected == null) return;

        var slider = selected.GetComponent<Slider>();

        if (slider == null) return;

        _currentSliderValue = slider.value;
        
        switch (volumeType)
        {
            case 0: // master
                dataHolder.masterVolume = _currentSliderValue;
                _audioManager.masterVolume = _currentSliderValue;
                break;
            case 2: // music
                dataHolder.musicVolume = _currentSliderValue;
                _audioManager.musicVolume = _currentSliderValue;
                _audioManager.ambienceVolume = _currentSliderValue;
                break;
            case 3: // sfx
                dataHolder.sfxVolume = _currentSliderValue;
                _audioManager.sfxVolume = _currentSliderValue;
                _audioManager.uiSfxVolume = _currentSliderValue;
                break;
        }
    }

    public void ForceControlScheme()
    {
        switch (controlSchemeDropdown.value)
        {
            case 0: // automatically detect control scheme
                dataHolder.forceControlScheme = false;
                break;
            case 1: // force control scheme to be Xbox
                dataHolder.forceControlScheme = true;
                dataHolder.currentControl = ControlsManager.ControlScheme.Xbox;
                break;
            case 2: // force control scheme to be PlayStation
                dataHolder.forceControlScheme = true;
                dataHolder.currentControl = ControlsManager.ControlScheme.Playstation;
                break;
            case 3: // force control scheme to be Keyboard and Mouse
                dataHolder.forceControlScheme = true;
                dataHolder.currentControl = ControlsManager.ControlScheme.Keyboard;
                break;
        }
    }
}
