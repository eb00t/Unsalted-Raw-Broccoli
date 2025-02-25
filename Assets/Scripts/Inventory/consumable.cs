using UnityEngine;

public class Consumable : MonoBehaviour
{
	public ConsumableEffect consumableEffect;
	
	public string title;
	[TextArea(3, 10)]
	public string description; // 191 characters is the absolute maximum
	public Sprite uiIcon;
}
