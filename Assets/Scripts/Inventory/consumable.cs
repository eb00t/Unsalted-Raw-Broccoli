using UnityEngine;

public class Consumable : MonoBehaviour
{
	public ConsumableEffect consumableEffect;
	public float effectDuration; // duration in seconds
	public float effectAmount;
	
	public string title;
	public Sprite uiIcon;
	public Sprite statusIcon, statusIcon2;
	public Color statusColor;
	public string statusText, statusText2;
	public bool isInstantUse;
	public int maximumHold;

	public int itemID;
	
	[TextArea(3, 10)]
	public string description;
}
