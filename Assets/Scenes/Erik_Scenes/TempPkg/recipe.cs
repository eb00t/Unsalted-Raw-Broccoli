using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Recipe", menuName = "ScriptableObjects/newRecipe", order = 1)]
	public class recipe : ScriptableObject
	{
		public string title;
		public List<Items> ingredients;
	}