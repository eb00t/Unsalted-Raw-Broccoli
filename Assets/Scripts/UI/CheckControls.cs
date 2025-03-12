using System;
using UnityEngine;

public class CheckControls : MonoBehaviour
{
    private bool _isGamepad;
    [SerializeField] private GameObject xboximg, psimg, keyimg;
    private GameObject _player;
    private ItemPickupHandler _itemPickupHandler;

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        _itemPickupHandler = _player.GetComponent<ItemPickupHandler>();
        UpdateImg();
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        UpdateImg();
    }

    private void UpdateImg()
    {
        _itemPickupHandler.CheckControl();
        switch (_itemPickupHandler.currentControl)
        {
            case ItemPickupHandler.ControlScheme.None:
            case ItemPickupHandler.ControlScheme.Xbox:
                xboximg.SetActive(true);
                psimg.SetActive(false);
                keyimg.SetActive(false);
                break;
            case ItemPickupHandler.ControlScheme.Playstation:
                xboximg.SetActive(false);
                psimg.SetActive(true);
                keyimg.SetActive(false);
                break;
            case ItemPickupHandler.ControlScheme.Keyboard:
                xboximg.SetActive(false);
                psimg.SetActive(false);
                keyimg.SetActive(true);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
