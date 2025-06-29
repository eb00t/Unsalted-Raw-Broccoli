using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

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

    public bool CheckIfSaveFileExists()
    {
        var filePath = Application.persistentDataPath + "/saveData.json";

        if (!File.Exists(filePath)) return false;

        var defaultData = new DataHolderSaveData();
        var json = File.ReadAllText(filePath);
        var loadedData = JsonUtility.FromJson<DataHolderSaveData>(json);

        return defaultData.currentLevel != loadedData.currentLevel ||
               defaultData.currencyHeld != loadedData.currencyHeld ||
               defaultData.highestFloorCleared != loadedData.highestFloorCleared ||
               !Mathf.Approximately(defaultData.playerTimeToClear, loadedData.playerTimeToClear);
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
            fpsIndex = dataHolder.fpsIndex,
            resolutionIndex = dataHolder.resolutionIndex,
            hasBeatenBaseGame = dataHolder.hasBeatenBaseGame,

            masterVolume = dataHolder.masterVolume,
            musicVolume = dataHolder.musicVolume,
            sfxVolume = dataHolder.sfxVolume,
            screenShakeMultiplier = dataHolder.screenShakeMultiplier,

            savedItems = new List<int>(dataHolder.savedItems),
            savedItemCounts = new List<int>(dataHolder.savedItemCounts),
            equippedConsumables = (int[])dataHolder.equippedConsumables.Clone(),
            permanentPassiveItems = (int[])dataHolder.permanentPassiveItems.Clone(),
            
            totalEnemiesKilled = dataHolder.totalEnemiesKilled,
            totalDeaths = dataHolder.totalDeaths,
            totalCoilsCollected = dataHolder.totalCoilsCollected,
            fastestClearTime = dataHolder.fastestClearTime,
            
            playerDeaths = dataHolder.playerDeaths,
            playerCoilsCollected = dataHolder.playerCoilsCollected,
            playerEnemiesKilled = dataHolder.playerEnemiesKilled,
            playerPersonalBestTime = dataHolder.playerPersonalBestTime,
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
            CreateDefaultSaveFile();
        }

        var json = File.ReadAllText(filePath);
        var loadedData = JsonUtility.FromJson<DataHolderSaveData>(json);

        dataHolder.currencyHeld = loadedData.currencyHeld;
        dataHolder.currentLevel = (LevelBuilder.LevelMode)loadedData.currentLevel;
        dataHolder.highestFloorCleared = loadedData.highestFloorCleared;
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
        
        dataHolder.savedItems = new List<int>(loadedData.savedItems);
        dataHolder.savedItemCounts = new List<int>(loadedData.savedItemCounts);
        dataHolder.equippedConsumables = (int[])loadedData.equippedConsumables.Clone();
        dataHolder.permanentPassiveItems = (int[])loadedData.permanentPassiveItems.Clone();
        
        dataHolder.hasBeatenBaseGame = loadedData.hasBeatenBaseGame;
        dataHolder.currentControl = (ControlsManager.ControlScheme)loadedData.currentControl;
        dataHolder.isAutoEquipEnabled = loadedData.isAutoEquipEnabled;
        dataHolder.isAutoSwitchEnabled = loadedData.isAutoSwitchEnabled;
        dataHolder.isAutoLockOnEnabled = loadedData.isAutoLockOnEnabled;
        dataHolder.forceControlScheme = loadedData.forceControlScheme;
        dataHolder.isGamepad = loadedData.isGamepad;
        dataHolder.fpsIndex = loadedData.fpsIndex;
        dataHolder.resolutionIndex = loadedData.resolutionIndex;
        dataHolder.masterVolume = loadedData.masterVolume;
        dataHolder.musicVolume = loadedData.musicVolume;
        dataHolder.sfxVolume = loadedData.sfxVolume;
        dataHolder.screenShakeMultiplier = loadedData.screenShakeMultiplier;
        
        dataHolder.totalEnemiesKilled = loadedData.totalEnemiesKilled;
        dataHolder.totalDeaths = loadedData.totalDeaths;
        dataHolder.totalCoilsCollected = loadedData.totalCoilsCollected;
        dataHolder.fastestClearTime = loadedData.fastestClearTime;
        
        dataHolder.playerEnemiesKilled = loadedData.playerEnemiesKilled;
        dataHolder.playerDeaths = loadedData.playerDeaths;
        dataHolder.playerCoilsCollected = loadedData.playerCoilsCollected;
        dataHolder.playerPersonalBestTime = loadedData.playerPersonalBestTime;
    }

    public void EraseData(bool keepSettings, bool keepGlobal)
    {
        DataHolderSaveData preservedSettings = null;
        DataHolderSaveData preservedGlobal = null;

        if (keepSettings)
        {
            preservedSettings = new DataHolderSaveData
            {
                hasBeatenBaseGame = dataHolder.hasBeatenBaseGame,
                currentControl = (int)dataHolder.currentControl,
                isAutoEquipEnabled = dataHolder.isAutoEquipEnabled,
                isAutoSwitchEnabled = dataHolder.isAutoSwitchEnabled,
                isAutoLockOnEnabled = dataHolder.isAutoLockOnEnabled,
                forceControlScheme = dataHolder.forceControlScheme,
                isGamepad = dataHolder.isGamepad,
                fpsIndex = dataHolder.fpsIndex,
                resolutionIndex = dataHolder.resolutionIndex,
                masterVolume = dataHolder.masterVolume,
                musicVolume = dataHolder.musicVolume,
                sfxVolume = dataHolder.sfxVolume,
                screenShakeMultiplier = dataHolder.screenShakeMultiplier,
                playerDeaths = dataHolder.playerDeaths,
                playerCoilsCollected = dataHolder.playerCoilsCollected,
                playerEnemiesKilled = dataHolder.playerEnemiesKilled,
                playerPersonalBestTime = dataHolder.playerPersonalBestTime,
            };
        }

        if (keepGlobal)
        {
            preservedGlobal = new DataHolderSaveData
            {
                fastestClearTime = dataHolder.fastestClearTime,
                totalEnemiesKilled = dataHolder.totalEnemiesKilled,
                totalDeaths = dataHolder.totalDeaths,
                totalCoilsCollected = dataHolder.totalCoilsCollected,
            };
        }

        File.Delete(Application.persistentDataPath + "/saveData.json");
        CreateDefaultSaveFile();
        dataHolder.playerTimeToClear = 0f;
        LoadData(dataHolder);
        
        if (keepSettings)
        {
            dataHolder.hasBeatenBaseGame = preservedSettings.hasBeatenBaseGame;
            dataHolder.currentControl = (ControlsManager.ControlScheme)preservedSettings.currentControl;
            dataHolder.isAutoEquipEnabled = preservedSettings.isAutoEquipEnabled;
            dataHolder.isAutoSwitchEnabled = preservedSettings.isAutoSwitchEnabled;
            dataHolder.isAutoLockOnEnabled = preservedSettings.isAutoLockOnEnabled;
            dataHolder.forceControlScheme = preservedSettings.forceControlScheme;
            dataHolder.isGamepad = preservedSettings.isGamepad;
            dataHolder.fpsIndex = preservedSettings.fpsIndex;
            dataHolder.resolutionIndex = preservedSettings.resolutionIndex;
            dataHolder.masterVolume = preservedSettings.masterVolume;
            dataHolder.musicVolume = preservedSettings.musicVolume;
            dataHolder.sfxVolume = preservedSettings.sfxVolume;
            dataHolder.screenShakeMultiplier = preservedSettings.screenShakeMultiplier;
            dataHolder.playerDeaths = preservedSettings.playerDeaths;
            dataHolder.playerCoilsCollected = preservedSettings.playerCoilsCollected;
            dataHolder.playerEnemiesKilled = preservedSettings.playerEnemiesKilled;
            dataHolder.playerPersonalBestTime = preservedSettings.playerPersonalBestTime;
        }

        if (keepGlobal)
        {
            dataHolder.fastestClearTime = preservedGlobal.fastestClearTime;
            dataHolder.totalEnemiesKilled = preservedGlobal.totalEnemiesKilled;
            dataHolder.totalDeaths = preservedGlobal.totalDeaths;
            dataHolder.totalCoilsCollected = preservedGlobal.totalCoilsCollected;
        }

        SavePlayerData(dataHolder);
    }


    private void OnApplicationQuit()
    {
        SavePlayerData(dataHolder);
    }
    
    public static void CreateDefaultSaveFile()
    {
        var defaultData = new DataHolderSaveData
        {
            currencyHeld = 0,
            currentLevel = 1,
            highestFloorCleared = 0,
            eraseViewedLore = true,

            playerHealth = 250,
            playerMaxHealth = 250,
            playerBaseAttack = 10,
            playerDefense = 0,
            surviveLethalHit = false,
            passiveEnergyRegen = false,
            hpChanceOnKill = false,
            changeToRegen = 10,
            hpChanceHealPercentage = 5,
            
            currentControl = 2,
            isAutoEquipEnabled = true,
            isAutoSwitchEnabled = true,
            isAutoLockOnEnabled = true,
            forceControlScheme = false,
            isGamepad = false,
            fpsIndex = 0,
            resolutionIndex = 0,
            hasBeatenBaseGame = false,

            masterVolume = 0.7f,
            musicVolume = 1f,
            sfxVolume = 1f,
            screenShakeMultiplier = 0.7f,

            savedItems = new List<int>(),
            savedItemCounts = new List<int>(),
            equippedConsumables = new int[5],
            permanentPassiveItems = new int[4],
            
            totalCoilsCollected = 0,
            totalEnemiesKilled = 0,
            totalDeaths = 0,
            fastestClearTime = 0f,
            
            playerTimeToClear = 0f,
            playerDeaths = 0,
            playerCoilsCollected = 0,
            playerEnemiesKilled = 0,
        };

        var json = JsonUtility.ToJson(defaultData, true);
        var filePath = Application.persistentDataPath + "/saveData.json";
        File.WriteAllText(filePath, json);
    }
}

[System.Serializable]
public class DataHolderSaveData
{
    public int currencyHeld;
    public int currentLevel = 1;
    public int highestFloorCleared;
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
    public int fpsIndex;
    public int resolutionIndex;
    public bool hasBeatenBaseGame;

    public float masterVolume;
    public float musicVolume;
    public float sfxVolume;
    public float screenShakeMultiplier;

    public List<int> savedItems;
    public List<int> savedItemCounts;
    public int[] equippedConsumables;
    public int[] permanentPassiveItems;

    public float fastestClearTime;
    public int totalCoilsCollected;
    public int totalEnemiesKilled;
    public int totalDeaths;
    
    public int playerCoilsCollected;
    public int playerEnemiesKilled;
    public int playerDeaths;
    public float playerTimeToClear;
    public float playerPersonalBestTime;
}
