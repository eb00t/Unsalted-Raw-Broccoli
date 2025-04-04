using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class SettingManager : MonoBehaviour
{
    private InventoryStore _inventoryStore;
    [SerializeField] private DataHolder dataHolder;
    [SerializeField] private Slider masterSlider, musicSlider, sfxSlider;
    [SerializeField] private TMP_Dropdown controlSchemeDropdown;
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
        LoadVolume();
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

    private void LoadVolume()
    {
        _audioManager.masterVolume = dataHolder.masterVolume;
        _audioManager.ambienceVolume = dataHolder.musicVolume;
        _audioManager.musicVolume = dataHolder.musicVolume;
        _audioManager.sfxVolume = dataHolder.sfxVolume;
        _audioManager.uiSfxVolume = dataHolder.sfxVolume;
        
        masterSlider.value = dataHolder.masterVolume;
        musicSlider.value = dataHolder.musicVolume;
        sfxSlider.value = dataHolder.sfxVolume;
    }

    public void UpdateScreenShake()
    {
        _currentSliderValue = EventSystem.current.currentSelectedGameObject.GetComponentInParent<Slider>().value;

        screenShakeMultiplier = _currentSliderValue;
        dataHolder.screenShakeMultiplier = _currentSliderValue;
    }

    public void UpdateVolume(int volumeType)
    {
        // gets current selected gameobject (slider handle)
        _currentSliderValue = EventSystem.current.currentSelectedGameObject.GetComponentInParent<Slider>().value;
        
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
