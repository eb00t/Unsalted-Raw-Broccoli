using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/DataHolder", order = 1)]
public class DataHolder : ScriptableObject
{
    public int currencyHeld;
    public LevelBuilder.LevelMode currentLevel;
    public int highestFloorCleared;
    public bool demoMode; // Essentially a playtest mode, if this is true, it should erase all data when reset is called
    public bool eraseViewedLore; // Bool to allow all lore to respawn
    public bool hasPlayedThisSession;
    
    [Header("Player Stats")]
    public int playerHealth;
    public int playerMaxHealth;
    public int playerBaseAttack;
    public int playerDefense;
    public bool surviveLethalHit;
    public bool passiveEnergyRegen;
    public bool hpChanceOnKill;
    public int changeToRegen;
    public int hpChanceHealPercentage;
    
    [Header("Settings")]
    public ControlsManager.ControlScheme currentControl;
    public bool isAutoEquipEnabled;
    public bool isAutoSwitchEnabled;
    public bool isAutoLockOnEnabled;
    public bool forceControlScheme;
    public bool isGamepad;
    public bool hardcoreMode;
    public bool hasBeatenBaseGame;
    public int fpsIndex;
    public int resolutionIndex;
    
    [Header("Volume")]
    [Range(0, 1)] 
    public float masterVolume;
    [Range(0, 1)] 
    public float musicVolume;
    [Range(0, 1)] 
    public float sfxVolume;
    [Range(0, 1)] 
    public float screenShakeMultiplier;
    
    [Header("Inventory")]
    public List<int> savedItems = new List<int>();
    public List<int> savedItemCounts = new List<int>();  
    public int[] equippedConsumables = new int[5];
    public int[] permanentPassiveItems = new int[4];

    [Header("Generic Stats")] 
    public float fastestClearTime;
    public int totalCoilsCollected;
    public int totalEnemiesKilled;
    public int totalDeaths;
    
    public float playerTimeToClear;
    public float playerPersonalBestTime;
    public int playerCoilsCollected;
    public int playerEnemiesKilled;
    public int playerDeaths;
}