using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpdateButton : MonoBehaviour
{
    private GameObject _player;
    private ItemPickupHandler _itemPickupHandler;
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI text;
    
    [SerializeField] private string keyboardText, xboxText, psText;
    [SerializeField] private Sprite square, circle, x, triangle;
    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        _itemPickupHandler = _player.GetComponent<ItemPickupHandler>();
    }

    private void OnEnable()
    {
        switch (_itemPickupHandler.currentControl)
        {
            case ItemPickupHandler.ControlScheme.None:
            case ItemPickupHandler.ControlScheme.Xbox:
                text.text = xboxText;
                image.enabled = false;
                break;
            case ItemPickupHandler.ControlScheme.Playstation:
                image.enabled = true;
                    
                switch (psText)
                {
                    case "square":
                        image.sprite = square;
                        break;
                    case "circle":
                        image.sprite = circle;
                        break;
                    case "x":
                        image.sprite = x;
                        break;
                    case "triangle":
                        image.sprite = triangle;
                        break;
                }
                    
                text.text = "";
                break;
            case ItemPickupHandler.ControlScheme.Keyboard:
                image.enabled = false;
                text.text = keyboardText;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
