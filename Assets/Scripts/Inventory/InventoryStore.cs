using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryStore : MonoBehaviour
{
    [SerializeField] private Transform block;
    [SerializeField] private GameObject notifPrefab;
    [SerializeField] private GameObject notifHolder;
    public Transform grid;
    private ToolbarHandler _toolbarHandler;
    [SerializeField] private DataHolder dataHolder;
    [SerializeField] private ItemDatabase itemDatabase;
    private MenuHandler _menuHandler;
    [SerializeField] private Color notifPositiveColor, notifNegativeColor, iconPositiveColor, iconNegativeColor;

    private void Start()
    {
        _toolbarHandler = GetComponent<ToolbarHandler>();
        _menuHandler = GetComponent<MenuHandler>();
        LoadItems();
        RefreshList();
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
            
            var b = grid.GetComponentsInChildren<IndexHolder>()
                .FirstOrDefault(b => b.consumable.itemID == consumable.itemID);

            if (b == null) continue;
                
            if (b.numHeld < consumable.maximumHold)
            {
                b.numHeld++;
                UpdateStoredCount(consumable.itemID, b.numHeld);
                TriggerNotification(consumable.uiIcon, consumable.title, true);
                _toolbarHandler.UpdateToolbar();
                UpdateUI(b);
                        
                consumable.gameObject.SetActive(false);
                consumable.gameObject.GetComponent<ItemPickup>().canPickup = false;
            }
            else
            {
                TriggerNotification(consumable.uiIcon, "Maximum number of item held", false);
            }
            return;
        }


        // if the item did not exist in inventory already then a new inventory button is created
        dataHolder.savedItems.Add(consumable.itemID);
        dataHolder.savedItemCounts.Add(1);

        TriggerNotification(consumable.uiIcon, consumable.title, true);
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
            if (_menuHandler.shopGUI != null && _menuHandler.shopGUI.activeSelf) return;
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
            if (s.name == "Image")
            {
                s.sprite = indexHolder.consumable.uiIcon;
                s.GetComponentInChildren<TextMeshProUGUI>().text =
                    indexHolder.numHeld + "/" + indexHolder.consumable.maximumHold;
            }
        }
    }

    public void TriggerNotification(Sprite icon, string text, bool isPositive)
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

        if (notification != null)
        {
            foreach (var txt in notification.GetComponentsInChildren<TextMeshProUGUI>())
            {
                if (txt.name != "NotifCount") continue;
                var parsed = int.TryParse(txt.text, out var num) ? num : 0;
                txt.text = (++parsed).ToString();
            }
        }
        else
        {
            notification = Instantiate(notifPrefab, notifPrefab.transform.position, notifPrefab.transform.rotation, notifHolder.transform);

            foreach (var txt in notification.GetComponentsInChildren<TextMeshProUGUI>())
            {
                if (txt.name == "Message") txt.text = text;
                txt.color = isPositive ? notifPositiveColor : notifNegativeColor;
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
                }
            }
        }
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
