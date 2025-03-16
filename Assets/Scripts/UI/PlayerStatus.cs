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
        if (statuses.Count > 0)
        {
            // avoids multiple of same status
            foreach (var status in statuses)
            {
                if (status == null) continue;
                var sT = status.GetComponent<Consumable>();

                if (sT.consumableEffect != consumable.consumableEffect) continue;
                
                if (Mathf.Approximately(sT.effectAmount, consumable.effectAmount))
                {
                    Destroy(status);
                }
            }
        }
        
        var newStatus = Instantiate(statusPrefab, statusPrefab.position, statusPrefab.rotation, statusHolder);
        CopyComponent(newStatus.gameObject, consumable);
        statuses.Add(newStatus.gameObject);
        var statusTimer = newStatus.GetComponent<StatusTimer>(); 
        statusTimer.targetTime = consumable.effectDuration;
        newStatus.GetComponentInChildren<TextMeshProUGUI>().text = consumable.statusText;

        foreach (var i in newStatus.GetComponentsInChildren<Image>())
        {
            switch (i.name)
            {
                case "Fill":
                    i.sprite = consumable.statusIcon;
                    i.color = new Color(consumable.statusColor.r, consumable.statusColor.g, consumable.statusColor.b, 1f);
                    break;
                case "Background":
                    i.sprite = consumable.statusIcon;
                
                    Color.RGBToHSV(consumable.statusColor, out var h, out var s, out var v);
                    v = Mathf.Clamp01(v - 0.65f);
                    i.color = Color.HSVToRGB(h, s, v);
                    break;
            }
        }
        statusTimer.isTimerStarted = true;
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
