using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ItemPickupHandler : MonoBehaviour
{
    private Transform player;
    private CharacterMovement characterMovement;

    [Header("UI References")]
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private TextMeshProUGUI text, betweenTxtObj;
    [SerializeField] private UpdateButton updateButton1, updateButton2;

    [Header("Item Handling")] 
    public int itemCount;
    public bool isPlrNearShop, isPlrNearEnd, isPlrNearDialogue, isPlrNearLore, isPlayerNearRecharge;
    
    private ControlsManager controlsManager;
    [SerializeField] private DataHolder dataHolder;

    private bool isGamepad;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        characterMovement = player.GetComponent<CharacterMovement>();

        controlsManager = GameObject.FindGameObjectWithTag("UIManager").GetComponent<ControlsManager>();
    }

    private void Update()
    {
        var isNearOtherObject = isPlrNearEnd || isPlrNearDialogue || isPlrNearLore || isPlrNearShop || isPlayerNearRecharge;
        if (SceneManager.GetActiveScene().name == "Tutorial" || characterMovement.uiOpen || isNearOtherObject) return;
        
        itemCount = 0;
        ItemPickup itemPickup = null;
        foreach (var item in GameObject.FindGameObjectsWithTag("Item"))
        {
            itemPickup = item.GetComponent<ItemPickup>();
            if (itemPickup == null || !itemPickup.canPickup) continue;
            itemCount++;
        }

        if (itemCount == 1 && itemPickup != null && itemPickup.isPermanentPassive)
        {
            TogglePrompt("Pick up passive item", true,  ControlsManager.ButtonType.Interact, "", null);
        }
        
        switch (itemCount)
        {
            case 0:
                TogglePrompt("", false, ControlsManager.ButtonType.Interact, "", null);
                break;
            case 1:
                TogglePrompt("Pick up consumable item", true, ControlsManager.ButtonType.Interact, "",null);
                break;
            default:
                TogglePrompt("Pick up consumable items", true, ControlsManager.ButtonType.Interact, "", null);
                break;
        }
    }

    public void PickUpItem(InputAction.CallbackContext context)
    {
        if (!context.performed || characterMovement.uiOpen) return;
         
        foreach (var item in GameObject.FindGameObjectsWithTag("Item"))
        {
            var itemPickup = item.GetComponent<ItemPickup>();
            if (itemPickup == null || !itemPickup.canPickup) continue;
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.ItemPickup, transform.position);
            itemPickup.AddItemToInventory();
        }
    }

    public void TogglePrompt(string promptText, bool toggle, ControlsManager.ButtonType button, string betweenText, ControlsManager.ButtonType? button2)
    {
        if (toggle)
        {
            rectTransform.anchoredPosition = new Vector3(0, 100, 0);
            text.text = promptText;
            updateButton1.button = button;

            if (button2.HasValue)
            {
                updateButton2.button = button2.Value;
                betweenTxtObj.text = betweenText;
                betweenTxtObj.gameObject.SetActive(true);
                updateButton2.image.gameObject.SetActive(true);
                updateButton2.text.gameObject.SetActive(true);
                updateButton2.enabled = true;
            }
            else
            {
                updateButton2.image.gameObject.SetActive(false);
                updateButton2.text.gameObject.SetActive(false);
                betweenTxtObj.gameObject.SetActive(false);
                updateButton2.enabled = false;
            }
        }
        else
        {
            rectTransform.anchoredPosition = new Vector3(0, -100, 0);
            text.text = "";
        }
    }
}
