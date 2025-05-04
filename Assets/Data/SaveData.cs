using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveData : MonoBehaviour
{
    public static SaveData Instance { get; private set; }
    [SerializeField] private DataHolder dataHolder;
    private float _autosaveTimer;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one SaveData script in the scene.");
        }

        Instance = this;
    }

    // automatically saves every minute
    private void Update()
    {
        _autosaveTimer += Time.deltaTime;
        if (_autosaveTimer >= 60f)
        {
            SavePlayerData(dataHolder);
        }
    }

    public void UpdateSave()
    {
        SavePlayerData(dataHolder);
    }

    public void LoadSave()
    {
        LoadData(dataHolder);
    }

    public static void SavePlayerData(DataHolder dataHolder)
    {
        var saveData = new DataHolderSaveData
        {
            currencyHeld = dataHolder.currencyHeld,
            currentLevel = (int)dataHolder.currentLevel,
            highestFloorCleared = dataHolder.highestFloorCleared,
            demoMode = dataHolder.demoMode,
            eraseViewedLore = dataHolder.eraseViewedLore,

            playerHealth = dataHolder.playerHealth,
            playerMaxHealth = dataHolder.playerMaxHealth,
            playerBaseAttack = dataHolder.playerBaseAttack,
            playerDefense = dataHolder.playerDefense,
            surviveLethalHit = dataHolder.surviveLethalHit,
            passiveEnergyRegen = dataHolder.passiveEnergyRegen,
            hpChanceOnKill = dataHolder.hpChanceOnKill,
            changeToRegen = dataHolder.changeToRegen,
            hpChanceHealPercentage = dataHolder.hpChanceHealPercentage,

            currentControl = (int)dataHolder.currentControl,
            isAutoEquipEnabled = dataHolder.isAutoEquipEnabled,
            isAutoSwitchEnabled = dataHolder.isAutoSwitchEnabled,
            isAutoLockOnEnabled = dataHolder.isAutoLockOnEnabled,
            forceControlScheme = dataHolder.forceControlScheme,
            isGamepad = dataHolder.isGamepad,

            masterVolume = dataHolder.masterVolume,
            musicVolume = dataHolder.musicVolume,
            sfxVolume = dataHolder.sfxVolume,
            screenShakeMultiplier = dataHolder.screenShakeMultiplier,

            savedItems = new List<int>(dataHolder.savedItems),
            savedItemCounts = new List<int>(dataHolder.savedItemCounts),
            equippedConsumables = (int[])dataHolder.equippedConsumables.Clone(),
            permanentPassiveItems = (int[])dataHolder.permanentPassiveItems.Clone()
        };

        var json = JsonUtility.ToJson(saveData, true);
        var filePath = Application.persistentDataPath + "/saveData.json";
        File.WriteAllText(filePath, json);
    }
    
    public static void LoadData(DataHolder dataHolder)
    {
        var filePath = Application.persistentDataPath + "/saveData.json";
        
        if (!File.Exists(filePath))
        {
            SavePlayerData(dataHolder);
            return;
        }

        var json = File.ReadAllText(filePath);
        var loadedData = JsonUtility.FromJson<DataHolderSaveData>(json);

        dataHolder.currencyHeld = loadedData.currencyHeld;
        dataHolder.currentLevel = (LevelBuilder.LevelMode)loadedData.currentLevel;
        dataHolder.highestFloorCleared = loadedData.highestFloorCleared;
        dataHolder.demoMode = loadedData.demoMode;
        dataHolder.eraseViewedLore = loadedData.eraseViewedLore;

        dataHolder.playerHealth = loadedData.playerHealth;
        dataHolder.playerMaxHealth = loadedData.playerMaxHealth;
        dataHolder.playerBaseAttack = loadedData.playerBaseAttack;
        dataHolder.playerDefense = loadedData.playerDefense;
        dataHolder.surviveLethalHit = loadedData.surviveLethalHit;
        dataHolder.passiveEnergyRegen = loadedData.passiveEnergyRegen;
        dataHolder.hpChanceOnKill = loadedData.hpChanceOnKill;
        dataHolder.changeToRegen = loadedData.changeToRegen;
        dataHolder.hpChanceHealPercentage = loadedData.hpChanceHealPercentage;

        dataHolder.currentControl = (ControlsManager.ControlScheme)loadedData.currentControl;
        dataHolder.isAutoEquipEnabled = loadedData.isAutoEquipEnabled;
        dataHolder.isAutoSwitchEnabled = loadedData.isAutoSwitchEnabled;
        dataHolder.isAutoLockOnEnabled = loadedData.isAutoLockOnEnabled;
        dataHolder.forceControlScheme = loadedData.forceControlScheme;
        dataHolder.isGamepad = loadedData.isGamepad;

        dataHolder.masterVolume = loadedData.masterVolume;
        dataHolder.musicVolume = loadedData.musicVolume;
        dataHolder.sfxVolume = loadedData.sfxVolume;
        dataHolder.screenShakeMultiplier = loadedData.screenShakeMultiplier;

        dataHolder.savedItems = new List<int>(loadedData.savedItems);
        dataHolder.savedItemCounts = new List<int>(loadedData.savedItemCounts);
        dataHolder.equippedConsumables = (int[])loadedData.equippedConsumables.Clone();
        dataHolder.permanentPassiveItems = (int[])loadedData.permanentPassiveItems.Clone();
    }

    public void EraseData()
    {
        File.Delete(Application.persistentDataPath + "/saveData.json");
        dataHolder.currencyHeld = 0;
        dataHolder.currentLevel = LevelBuilder.LevelMode.Floor1;
        dataHolder.highestFloorCleared = 0;
        dataHolder.savedItems.Clear();
        dataHolder.savedItemCounts.Clear();
        dataHolder.equippedConsumables = new int[5];
        dataHolder.currencyHeld = 0;
        dataHolder.permanentPassiveItems = new int[4];
    }

    private void OnApplicationQuit()
    {
        SavePlayerData(dataHolder);
    }
}

[System.Serializable]
public class DataHolderSaveData
{
    public int currencyHeld;
    public int currentLevel;
    public int highestFloorCleared;
    public bool demoMode;
    public bool eraseViewedLore;

    public int playerHealth;
    public int playerMaxHealth;
    public int playerBaseAttack;
    public int playerDefense;
    public bool surviveLethalHit;
    public bool passiveEnergyRegen;
    public bool hpChanceOnKill;
    public int changeToRegen;
    public int hpChanceHealPercentage;

    public int currentControl;
    public bool isAutoEquipEnabled;
    public bool isAutoSwitchEnabled;
    public bool isAutoLockOnEnabled;
    public bool forceControlScheme;
    public bool isGamepad;

    public float masterVolume;
    public float musicVolume;
    public float sfxVolume;
    public float screenShakeMultiplier;

    public List<int> savedItems;
    public List<int> savedItemCounts;
    public int[] equippedConsumables;
    public int[] permanentPassiveItems;
}
