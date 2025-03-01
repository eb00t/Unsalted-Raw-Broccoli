using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatus : MonoBehaviour
{
    [SerializeField] private Transform statusHolder;
    [SerializeField] private Transform statusPrefab;
    public List<GameObject> statuses;

    public void AddNewStatus(Consumable consumable)
    {
        if (statuses.Count > 0)
        {
            foreach (var status in statuses) // makes sure timer resets if status is reapplied
            {
                Destroy(status);
            }
        }
        
        var newStatus = Instantiate(statusPrefab, statusPrefab.position, statusPrefab.rotation, statusHolder);
        statuses.Add(newStatus.gameObject);
        var statusTimer = newStatus.GetComponent<StatusTimer>(); 
        statusTimer.targetTime = consumable.effectDuration;
        statusTimer.consumableEffect = consumable.consumableEffect;

        foreach (var i in newStatus.GetComponentsInChildren<Image>())
        {
            if (i.name == "Fill")
            {
                i.sprite = consumable.statusIcon;
                i.color = new Color(consumable.statusColor.r, consumable.statusColor.g, consumable.statusColor.b, 1f);
            }
            else if (i.name == "Background")
            {
                i.sprite = consumable.statusIcon;
                
                Color.RGBToHSV(consumable.statusColor, out var h, out var s, out var v);
                v = Mathf.Clamp01(v - 0.65f);
                i.color = Color.HSVToRGB(h, s, v);
            }
        }
        statusTimer.isTimerStarted = true;
    }

    private void RefreshStatuses()
    {
        foreach (var n in statusHolder.GetComponentsInChildren<Transform>())
        {
            if (n != statusHolder)
            {
                Destroy(n.gameObject);
            }
        }

        foreach (var t in statuses)
        {
            AddNewStatus(t.GetComponent<Consumable>());
        }
    }
}
