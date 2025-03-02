using UnityEngine;

public class Consumable : MonoBehaviour
{
	public ConsumableEffect consumableEffect;
	public float effectDuration; // duration in seconds
	public float effectAmount;
	
	public string title;
	public Sprite uiIcon;
	public Sprite statusIcon;
	public Color statusColor;
	public string statusText;
	public bool isInstantUse;
	public int maximumHold;
	
	[TextArea(3, 10)]
	public string description;
	
}
