using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "consumable", menuName = "ScriptableObjects/consumables", order = 1)]
public class consumable : ScriptableObject
{
    public string title;
    //public List<Weapons> properties;
    public int maxHold;
    public int totalStored;
}
