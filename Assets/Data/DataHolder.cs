using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/DataHolder", order = 1)]
public class DataHolder : ScriptableObject
{
    public int currencyHeld;
    
    public ControlsManager.ControlScheme currentControl;
    
    public bool isAutoEquipEnabled;
    public bool isAutoSwitchEnabled;
    public bool forceControlScheme;
    public bool isGamepad;
}