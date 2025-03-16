using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    private InventoryStore _inventoryStore;
    [SerializeField] private DataHolder dataHolder;
    [SerializeField] private Slider masterSlider, musicSlider, sfxSlider;
    private float _currentSlider;
    private AudioManager _audioManager;
    
    private void Start()
    {
        _audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
        LoadVolume();
    }

    public void ToggleAutoEquip()
    {
        dataHolder.isAutoEquipEnabled = !dataHolder.isAutoEquipEnabled;
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
        _audioManager.ambienceVolume = dataHolder.ambientVolume;
        _audioManager.musicVolume = dataHolder.musicVolume;
        _audioManager.sfxVolume = dataHolder.sfxVolume;
        _audioManager.uiSfxVolume = dataHolder.uiVolume;
        
        masterSlider.value = dataHolder.masterVolume;
        musicSlider.value = dataHolder.musicVolume;
        sfxSlider.value = dataHolder.sfxVolume;
    }

    public void UpdateVolume(int volumeType)
    {
        // gets current selected gameobject (slider handle)
        _currentSlider = EventSystem.current.currentSelectedGameObject.GetComponentInParent<Slider>().value;
        
        switch (volumeType)
        {
            case 0: // master
                dataHolder.masterVolume = _currentSlider;
                _audioManager.masterVolume = _currentSlider;
                break;
            case 1: // ambience
                dataHolder.ambientVolume = _currentSlider;
                _audioManager.ambienceVolume = _currentSlider;
                break;
            case 2: // music
                dataHolder.musicVolume = _currentSlider;
                _audioManager.musicVolume = _currentSlider;
                break;
            case 3: // sfx
                dataHolder.sfxVolume = _currentSlider;
                _audioManager.sfxVolume = _currentSlider;
                break;
            case 4: // hud sfx
                dataHolder.uiVolume = _currentSlider;
                _audioManager.uiSfxVolume = _currentSlider;
                break;
        }
    }

    public void ForceControlScheme(int controlScheme)
    {
        switch (controlScheme)
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
