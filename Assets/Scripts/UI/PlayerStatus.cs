using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatus : MonoBehaviour
{
    [SerializeField] private Transform statusHolder;
    [SerializeField] private Transform statusPrefab;
    public List<GameObject> statuses;

    public void AddNewStatus(Consumable consumable)
    {
        // avoids multiple of same status
        for (var i = statuses.Count - 1; i >= 0; i--)
        {
            if (statuses[i] == null) continue;

            var existing = statuses[i].GetComponent<Consumable>();

            if (existing == null) continue;
            if (existing.consumableEffect != consumable.consumableEffect) continue;
            if (!Mathf.Approximately(existing.effectAmount, consumable.effectAmount)) continue;
            
            Destroy(statuses[i]);
            statuses.RemoveAt(i);
        }
        
        AddStatus(consumable, consumable.statusIcon, consumable.statusText);
        
        if (consumable.statusIcon2 != null)
        {
            AddStatus(consumable, consumable.statusIcon2, consumable.statusText2);
        }
    }
    
    private void AddStatus(Consumable consumable, Sprite icon, string statusText)
    {
        var newStatus = Instantiate(statusPrefab, statusPrefab.position, statusPrefab.rotation, statusHolder);
        CopyComponent(newStatus.gameObject, consumable);
        statuses.Add(newStatus.gameObject);

        var statusTimer = newStatus.GetComponent<StatusTimer>();
        statusTimer.targetTime = consumable.effectDuration;
        statusTimer.isTimerStarted = true;

        statusTimer.GetComponentInChildren<TextMeshProUGUI>().text = statusText;

        foreach (var image in newStatus.GetComponentsInChildren<Image>())
        {
            if (image.GetComponent<StatusTimer>())
            {
                image.color = new Color(consumable.statusColor.r, consumable.statusColor.g, consumable.statusColor.b, 1f);
            }
            
            if (image.name == "Fill")
            {
                image.sprite = icon;
                image.color = new Color(consumable.statusColor.r, consumable.statusColor.g, consumable.statusColor.b, 1f);
            }
            
            if (image.name == "Background")
            {
                image.sprite = icon;
                Color.RGBToHSV(consumable.statusColor, out var h, out var s, out var v);
                v = Mathf.Clamp01(v - 0.25f);
                image.color = Color.HSVToRGB(h, s, v);
            }
        }
    }

    // copies a consumable component to the specified gameobject
    private void CopyComponent(GameObject destination, Consumable consumable)
    {
        var copy = destination.AddComponent<Consumable>();
        
        foreach (var field in consumable.GetType().GetFields())
        {
            field.SetValue(copy, field.GetValue(consumable));
        }

        foreach (var property in consumable.GetType().GetProperties())
        {
            if (property.CanWrite)
            {
                property.SetValue(copy, property.GetValue(consumable));
            }
        }
    }

    public void UpdateStatuses(int amount)
    {
        if (statuses.Count <= 0) return;
        foreach (var s in statuses)
        {
            if (s == null) continue;
            if (s.GetComponent<Consumable>().consumableEffect != ConsumableEffect.Invincibility) continue;
            if (amount > 0)
            {
                s.GetComponentInChildren<TextMeshProUGUI>().text = amount.ToString();
            }
            else
            {
                Destroy(s);
            }
        }
    }
}
