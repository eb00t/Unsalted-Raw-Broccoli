using UnityEngine;

public class Consumable : MonoBehaviour
{
	public ConsumableEffect consumableEffect;
	
	public string title;
	[TextArea(3, 10)]
	public string description; // 79 characters is the absolute maximum
	public Sprite uiIcon;
}
