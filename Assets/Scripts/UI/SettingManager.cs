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
    [SerializeField] private TMP_Dropdown controlSchemeDropdown, fpsDropdown, resolutionDropdown;
    [SerializeField] private Toggle autoEquipToggle, autoLockOnToggle, autoSwitchToggle;
    [Range(0, 1)]
    public float screenShakeMultiplier = 1;
    private float _currentSlider;
    private float _currentSliderValue;
    private AudioManager _audioManager;
    private GameObject _lastSelected;
    
    private readonly Vector2Int[] _resolutions = {
        // 16:9
        new (1280, 720),
        new (1920, 1080),
        new (2560, 1440),
        new (3840, 2160),
        // 16:10
        new (1920, 1200),
        new (2560, 1600),
        new (3840, 2400)
    };
    
    private readonly int[] _fps = {
        -1, // uncapped
        30,
        60,
        90,
        120,
        144,
        165,
        240
    };
    
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
        SaveData.Instance.UpdateSave();
    }

    public void ToggleAutoLockOn()
    {
        dataHolder.isAutoLockOnEnabled = !dataHolder.isAutoLockOnEnabled;
        ButtonHandler.Instance.PlayConfirmSound();
        SaveData.Instance.UpdateSave();
    }

    public void ToggleAutoSwitchLockTarget()
    {
        dataHolder.isAutoSwitchEnabled = !dataHolder.isAutoSwitchEnabled;
        ButtonHandler.Instance.PlayConfirmSound();
        SaveData.Instance.UpdateSave();
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
        
        SetResolution(dataHolder.resolutionIndex);
        resolutionDropdown.value = dataHolder.resolutionIndex;
        SetFPS(dataHolder.fpsIndex);
        fpsDropdown.value = dataHolder.fpsIndex;
    }

    public void SetFPS(int index)
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = _fps[index];
        dataHolder.fpsIndex = index;
        SaveData.Instance.UpdateSave();
    }
    
    public void SetResolution(int index)
    {
        if (index == 0)
        {
            var res = Screen.currentResolution;
            Screen.SetResolution(res.width, res.height, FullScreenMode.ExclusiveFullScreen);
            resolutionDropdown.options[0].text = res.width + " x " + res.height;
        }
        else
        {
           var resolution = _resolutions[index];
           Screen.SetResolution(resolution.x, resolution.y, FullScreenMode.ExclusiveFullScreen);
           dataHolder.resolutionIndex = index; 
        }
        
        SaveData.Instance.UpdateSave();
    }

    public void UpdateScreenShake()
    {
        screenShakeMultiplier = screenShakeSlider.value;
        dataHolder.screenShakeMultiplier = screenShakeSlider.value;
        SaveData.Instance.UpdateSave();
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
        
        SaveData.Instance.UpdateSave();
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
        
        SaveData.Instance.UpdateSave();
    }
}
