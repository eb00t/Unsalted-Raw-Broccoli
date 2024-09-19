using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Crafting : MonoBehaviour
{
	[SerializeField] private GameObject left, right, up, block, final;
	[SerializeField] private List<recipe> recipes;

	public void AttemptCraft()
	{
		var l = left.GetComponentInChildren<DragDropUI>();
		var r = right.GetComponentInChildren<DragDropUI>();
		var u = up.GetComponentInChildren<DragDropUI>();

		if (l == null || r == null || u == null) return;
		if (CompareToRecipe(l, r, u) != null)
		{
			var rec = CompareToRecipe(l, r, u);
			if (rec == null) return;
			//_inventoryStore.AddNewItem(Enum.Parse<Items>(rec.title));
			Destroy(l.gameObject);
			Destroy(r.gameObject);
			Destroy(u.gameObject);
			var b = Instantiate(block, final.transform.position, final.transform.rotation, final.transform);  // create new inventory item
			b.GetComponent<DragDropUI>().ingredient = Enum.Parse<Items>(rec.title); // set the new object's item to correct ingredient
			b.GetComponentInChildren<TextMeshProUGUI>().text = rec.title;
			
			/*
			foreach (var s in b.GetComponent<DragDropUI>().sprites)
			{
				if (s.name == b.GetComponent<DragDropUI>().ingredient.ToString())
				{
					b.GetComponentInChildren<Image>().sprite = s;
				}
			}
			*/
		}
	}

	private recipe CompareToRecipe(DragDropUI l, DragDropUI r, DragDropUI u)
	{
		foreach (var rec in recipes)
		{
			if (rec.ingredients.Contains(l.ingredient) && rec.ingredients.Contains(r.ingredient) && rec.ingredients.Contains(u.ingredient))
			{
				return rec;
			}
		}
		
		Debug.Log("null");
		return null;
	}
}

