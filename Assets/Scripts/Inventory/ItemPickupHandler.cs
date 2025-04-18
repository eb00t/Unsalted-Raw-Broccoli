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
    [SerializeField] private TextMeshProUGUI text;
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
        foreach (var item in GameObject.FindGameObjectsWithTag("Item"))
        {
            var itemPickup = item.GetComponent<ItemPickup>();
            if (itemPickup == null || !itemPickup.canPickup) continue;
            itemCount++;
        }
        
        switch (itemCount)
        {
            case 0:
                TogglePrompt("", false, ControlsManager.ButtonType.Interact, null);
                break;
            case 1:
                TogglePrompt("Pick up item", true, ControlsManager.ButtonType.Interact, null);
                break;
            default:
                TogglePrompt("Pick up items", true, ControlsManager.ButtonType.Interact, null);
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

    public void TogglePrompt(string promptText, bool toggle, ControlsManager.ButtonType button, ControlsManager.ButtonType? button2)
    {
        if (toggle)
        {
            rectTransform.anchoredPosition = new Vector3(0, 100, 0);
            text.text = promptText;
            updateButton1.button = button;

            if (button2.HasValue)
            {
                updateButton2.button = button2.Value;
                updateButton2.gameObject.SetActive(true);
            }
            else
            {
                updateButton2.gameObject.SetActive(false);
            }
        }
        else
        {
            rectTransform.anchoredPosition = new Vector3(0, -100, 0);
            text.text = "";
        }
    }
}
