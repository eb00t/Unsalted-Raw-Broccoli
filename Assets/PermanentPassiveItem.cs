using UnityEngine;

public class PermanentPassiveItem : MonoBehaviour
{
	public PassiveEffect passiveEffect;
    public float effectAmount;
	
    public string title;
    
    public Sprite uiIcon;
    public Sprite statusIcon, statusIcon2;
    public Color statusColor;
    public string statusText, statusText2;

    public int itemID;
	
    [TextArea(3, 10)]
    public string description;
}