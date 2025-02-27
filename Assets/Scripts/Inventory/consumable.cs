using UnityEngine;

public class Consumable : MonoBehaviour
{
	public ConsumableEffect consumableEffect;
	
	public string title;
	public Sprite uiIcon;
	public bool isInstantUse;
	public int maximumHold;
	
	[TextArea(3, 10)]
	public string description;
}
