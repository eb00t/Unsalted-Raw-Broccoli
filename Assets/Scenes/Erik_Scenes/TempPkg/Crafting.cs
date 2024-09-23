using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Crafting : MonoBehaviour
{
	[SerializeField] private GameObject left, right, up, block, final;
	[SerializeField] private List<consumable> consumables;

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
		}
	}

	private consumable CompareToRecipe(DragDropUI l, DragDropUI r, DragDropUI u)
	{
		foreach (var con in consumables)
		{
			if (con.properties.Contains(l.ingredient) && con.properties.Contains(r.ingredient) && con.properties.Contains(u.ingredient))
			{
				return con;
			}
		}
		
		Debug.Log("null");
		return null;
	}
}

