using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/DataHolder", order = 1)]
public class DataHolder : ScriptableObject
{
    public int currencyHeld;
    public LevelBuilder.LevelMode currentLevel;
    
    [Header("Settings")]
    public ControlsManager.ControlScheme currentControl;
    public bool isAutoEquipEnabled;
    public bool isAutoSwitchEnabled;
    public bool forceControlScheme;
    public bool isGamepad;
    
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
    public List<int> equippedConsumables = new List<int>();
}