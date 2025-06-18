using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

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
        var nearbyItems = GameObject.FindGameObjectsWithTag("Item");
        var isNearPassive = false;
        
        foreach (var item in nearbyItems)
        {
            var itemPickup = item.GetComponent<ItemPickup>();
            if (itemPickup == null || !itemPickup.canPickup) continue;
            isNearPassive = itemPickup.isPermanentPassive;
            itemCount++;
        }

        if (isNearPassive)
        {
            TogglePrompt("Pick up passive item", true,  ControlsManager.ButtonType.Interact, "", null, false);
            return;
        }
        
        switch (itemCount)
        {
            case 0:
                TogglePrompt("", false, ControlsManager.ButtonType.Interact, "", null, false);
                break;
            case 1:
                TogglePrompt("Pick up consumable", true, ControlsManager.ButtonType.Interact, "",null, false);
                break;
            default:
                TogglePrompt("Pick up consumables", true, ControlsManager.ButtonType.Interact, "", null, false);
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

    public void TogglePrompt(string promptText, bool toggle, ControlsManager.ButtonType button, string betweenText, ControlsManager.ButtonType? button2, bool forceTween)
    {
        if (toggle)
        {
            if (rectTransform.anchoredPosition.y < 0 || forceTween) // animate
            {
                if (forceTween)
                {
                    rectTransform.DOScale(new Vector3(0, 1, 1), .1f).SetUpdate(true).OnComplete(() =>
                    {
                        rectTransform.anchoredPosition = new Vector3(0, 100, 0);
                        rectTransform.localScale = new Vector3(0, 1, 1);
                        rectTransform.DOScale(new Vector3(1, 1, 1), .1f).SetUpdate(true);
                    });
                }
                else
                {
                    rectTransform.anchoredPosition = new Vector3(0, 100, 0);
                    rectTransform.localScale = new Vector3(0, 1, 1);
                    rectTransform.DOScale(new Vector3(1, 1, 1), .1f).SetUpdate(true);
                }
            }
            
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
            if (rectTransform.anchoredPosition.y > 0 || forceTween) // animate
            {
                text.text = "";
                rectTransform.DOScale(new Vector3(0, 1, 1), .1f).SetUpdate(true).OnComplete(() =>
                {
                    rectTransform.anchoredPosition = new Vector3(0, -100, 0);
                });
            }
        }
    }
}
