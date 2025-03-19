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
    [SerializeField] private UpdateButton updateButton;

    [Header("Item Handling")] 
    public bool isPlrNearShop, isPlrNearEnd, isPlrNearDialogue, isPlrNearDialogue1;
    public int itemCount;
    
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
        if (SceneManager.GetActiveScene().name == "Tutorial" || characterMovement.uiOpen || isPlrNearShop || isPlrNearEnd || isPlrNearDialogue) return;
        
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
                TogglePrompt("", false, ControlsManager.ButtonType.ButtonEast);
                break;
            case 1:
                TogglePrompt("Pick Up Item", true, ControlsManager.ButtonType.ButtonEast);
                break;
            default:
                TogglePrompt("Pick Up Items", true, ControlsManager.ButtonType.ButtonEast);
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

    public void TogglePrompt(string promptText, bool toggle, ControlsManager.ButtonType button)
    {
        if (toggle)
        {
            rectTransform.anchoredPosition = new Vector3(0, 100, 0);
            text.text = promptText;
            updateButton.button = button;
        }
        else
        {
            rectTransform.anchoredPosition = new Vector3(0, -100, 0);
            text.text = "";
        }
    }
}
