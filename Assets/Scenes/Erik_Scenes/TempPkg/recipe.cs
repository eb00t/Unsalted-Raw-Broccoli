using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Consumable", menuName = "ScriptableObjects/consumables", order = 1)]
	public class consumable : ScriptableObject
	{
		public string title;
		public List<Items> properties;
	}