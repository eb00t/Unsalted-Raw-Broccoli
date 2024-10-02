using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "ScriptableObjects/weapons", order = 1)]
	public class WeaponType : ScriptableObject
	{
		public string title;
		public List<Weapons> properties;
		public int maxHold;
		public int totalStored;
	}