using System;
using System.Linq;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryStore : MonoBehaviour
{
    [SerializeField] private Transform block;
    [SerializeField] private GameObject notifPrefab;
    [SerializeField] private GameObject notifHolder;
    [SerializeField] private TextMeshProUGUI infoTxt, numHeldTxt, infoTitle;
    public Transform grid;
    private ToolbarHandler _toolbarHandler;
    [SerializeField] private DataHolder dataHolder;
    [SerializeField] private ItemDatabase itemDatabase;
    private MenuHandler _menuHandler;
    [SerializeField] private Color notifPositiveColor, notifNegativeColor, iconPositiveColor, iconNegativeColor;
    private GameObject _lastSelected;

    private void Start()
    {
        _toolbarHandler = GetComponent<ToolbarHandler>();
        _menuHandler = GetComponent<MenuHandler>();
        LoadItems();
        RefreshList();
    }

    private void Update()
    {
        if (EventSystem.current.currentSelectedGameObject == _lastSelected) return;
        _lastSelected = EventSystem.current.currentSelectedGameObject;
        UpdateInfo();
    }

    private void UpdateInfo()
    {
        if (EventSystem.current.currentSelectedGameObject == null) return;
        
        var indexHolder = EventSystem.current.currentSelectedGameObject.GetComponentInChildren<IndexHolder>();

        if (indexHolder == null || indexHolder.consumable == null) return;
        
        infoTxt.text = indexHolder.consumable.description;
        infoTitle.text = indexHolder.consumable.title;
        numHeldTxt.text = indexHolder.numHeld.ToString();
        //infoImg.sprite = indexHolder.consumable.uiIcon;
    }

    private void LoadItems()
    {
        for (var i = 0; i < dataHolder.savedItems.Count; i++)
        {
            var itemID = dataHolder.savedItems[i];
            var itemCount = dataHolder.savedItemCounts[i];

            var item = FindConsumable(itemID);
            if (item != null)
            {
                LoadUI(item, itemCount);
            }
        }
    }
    
    private void LoadUI(Consumable item, int itemCount)
    {
        var newBlock = Instantiate(block, block.position, block.rotation, grid);
        newBlock.GetComponentInChildren<TextMeshProUGUI>().text = item.title;
        var indexHolder = newBlock.GetComponent<IndexHolder>();
        indexHolder.consumable = item;
        indexHolder.numHeld = itemCount;

        newBlock.GetComponent<Button>().onClick.AddListener(delegate
        {
            _toolbarHandler.InvItemSelected(item);
        });

        UpdateUI(indexHolder);
    }
    
    public Consumable FindConsumable(int itemID)
    {
        return itemDatabase.allItems.FirstOrDefault(item => item.itemID == itemID);
    }

    private void RefreshList()
    {
        foreach (var n in grid.GetComponentsInChildren<Transform>())
        {
            if (n != grid)
            {
                Destroy(n.gameObject);
            }
        }
        
        for (var i = 0; i < dataHolder.savedItems.Count; i++)
        {
            var itemID = dataHolder.savedItems[i];
            var itemCount = dataHolder.savedItemCounts[i];
            var item = FindConsumable(itemID);
            if (item != null)
            {
                LoadUI(item, itemCount);
            }
        }
    }

    // checks if item added exists in inventory, if so it increases the number held and returns
   public void AddNewItem(Consumable consumable)
    {
        foreach (var t in dataHolder.savedItems)
        {
            if (t != consumable.itemID) continue;
            
            var b = grid.GetComponentsInChildren<IndexHolder>().FirstOrDefault(b => b.consumable.itemID == consumable.itemID);

            if (b == null) continue;
                
            if (b.numHeld < consumable.maximumHold || LevelBuilder.Instance.currentFloor == LevelBuilder.LevelMode.Tutorial)
            {
                b.numHeld++;
                UpdateStoredCount(consumable.itemID, b.numHeld);
                TriggerNotification(consumable.uiIcon, consumable.title, true, 2f);
                _toolbarHandler.UpdateToolbar();
                UpdateUI(b);
                        
                consumable.gameObject.SetActive(false);
                consumable.gameObject.GetComponent<ItemPickup>().canPickup = false;
            }
            else
            {
                TriggerNotification(consumable.uiIcon, "Maximum number of item held", false, 2f);
            }
            return;
        }


        // if the item did not exist in inventory already then a new inventory button is created
        dataHolder.savedItems.Add(consumable.itemID);
        dataHolder.savedItemCounts.Add(1);

        TriggerNotification(consumable.uiIcon, consumable.title, true, 2f);
        consumable.gameObject.SetActive(false);
        consumable.gameObject.GetComponent<ItemPickup>().canPickup = false;

        var newBlock = Instantiate(block, block.position, block.rotation, grid);
        newBlock.GetComponentInChildren<TextMeshProUGUI>().text = consumable.title;

        var indexHolder = newBlock.GetComponent<IndexHolder>();
        indexHolder.consumable = consumable;
        indexHolder.numHeld = 1;
        
        // updates the onclick for the new inventory item button
        newBlock.GetComponent<Button>().onClick.AddListener(delegate { _toolbarHandler.InvItemSelected(consumable); });

        UpdateUI(indexHolder);

        if (dataHolder.isAutoEquipEnabled)
        {
            //if (_menuHandler.shopGUI != null && _menuHandler.shopGUI.activeSelf) return;
            _toolbarHandler.AddToToolbar(consumable);
        }
    }

    private void UpdateStoredCount(int itemID, int newCount)
    {
        var index = dataHolder.savedItems.IndexOf(itemID);

        if (index != -1)
        {
            dataHolder.savedItemCounts[index] = newCount;
        }
    }

    private void UpdateUI(IndexHolder indexHolder)
    {
        foreach (var s in indexHolder.GetComponentsInChildren<Image>())
        {
            if (s.name == "Holder")
            {
                s.GetComponentInChildren<TextMeshProUGUI>().text = indexHolder.numHeld + "/" + indexHolder.consumable.maximumHold;
            }
            
            if (s.name == "Image") s.sprite = indexHolder.consumable.uiIcon;
                
        }
        
        _toolbarHandler.CheckEquipStatus();
    }

    public void TriggerNotification([CanBeNull] Sprite icon, string text, bool isPositive, float duration)
    {
        GameObject notification = null;

        foreach (var existing in notifHolder.GetComponentsInChildren<Transform>())
        {
            var txtMatch = existing.GetComponentsInChildren<TextMeshProUGUI>().Any(txt => txt.name == "Message" && txt.text == text);
            var iconMatch = existing.GetComponentsInChildren<Image>().Any(img => img.name == "IconImg");

            if (!txtMatch || !iconMatch) continue;
            notification = existing.gameObject;
            break;
        }

        // if this notification exists already find it and increment the count on that notification
        if (notification != null)
        {
            foreach (var txt in notification.GetComponentsInChildren<TextMeshProUGUI>())
            {
                if (txt.name == "NotifCount")
                {
                    var textToParse = txt.text.Split("x");
                    var parsed = false;
                    var num = 0;
                    
                    foreach (var s in textToParse)
                    {
                         parsed = int.TryParse(s, out num);
                    }
                    if (!parsed) num = 1;
                    
                    num++;
                    txt.text = "x" + num;
                }
            }
        }
        else
        {
            notification = Instantiate(notifPrefab, notifPrefab.transform.position, notifPrefab.transform.rotation, notifHolder.transform);
            if (duration > 0f)
            {
                notification.GetComponent<TimedDestroy>().notificationDuration = duration;
            }

            foreach (var txt in notification.GetComponentsInChildren<TextMeshProUGUI>())
            {
                switch (txt.name)
                {
                    case "Message":
                        txt.text = text;
                        break;
                    case "NotifCount":
                        txt.color = isPositive ? notifPositiveColor : notifNegativeColor;
                        break;
                }
            }

            foreach (var img in notification.GetComponentsInChildren<Image>())
            {
                switch (img.name)
                {
                    case "Image":
                        img.color = isPositive ? notifPositiveColor : notifNegativeColor;
                        break;
                    case "GameObject":
                        img.color = isPositive ? iconPositiveColor : iconNegativeColor;
                        break;
                    case "IconImg":
                        img.enabled = icon != null;
                        if (icon != null) img.sprite = icon;
                        break;
                    case "IconBck":
                        img.enabled = icon != null;
                        break;
                }
            }
        }
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(notifHolder.GetComponent<RectTransform>());
    }

    public void UpdateItemsHeld(Consumable consumable)
    {
        for (var i = 0; i < grid.GetComponentsInChildren<IndexHolder>().Length; i++)
        {
            var indexHolder = grid.GetComponentsInChildren<IndexHolder>()[i];

            if (indexHolder.consumable.title == consumable.title)
            {
                if (indexHolder.numHeld - 1 <= 0)
                {
                    var itemIndex = dataHolder.savedItems.IndexOf(consumable.itemID);
                    if (itemIndex != -1)
                    {
                        dataHolder.savedItems.RemoveAt(itemIndex);
                        dataHolder.savedItemCounts.RemoveAt(itemIndex);
                    }
                    Destroy(indexHolder.gameObject);
                }
                else
                {
                    indexHolder.numHeld--;
                    UpdateStoredCount(consumable.itemID, indexHolder.numHeld);

                    foreach (var img in indexHolder.GetComponentsInChildren<Image>())
                    {
                        if (img.name == "Image")
                        {
                            img.GetComponentInChildren<TextMeshProUGUI>().text = indexHolder.numHeld + "/" + indexHolder.consumable.maximumHold;
                        }
                    }
                }

                _toolbarHandler.UpdateToolbar();
                break;
            }
        }
    }
}
