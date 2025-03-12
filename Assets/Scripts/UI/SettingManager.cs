using Unity.VisualScripting;
using UnityEngine;

public class SettingManager : MonoBehaviour
{
    private GameObject _player;
    private GameObject _uiManager;
    private LockOnController _lockOnController;
    private InventoryStore _inventoryStore;
    private ItemPickupHandler _itemPickupHandler;

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        _lockOnController = _player.GetComponent<LockOnController>();
        _itemPickupHandler = _player.GetComponent<ItemPickupHandler>();
        
        _uiManager = GameObject.FindGameObjectWithTag("UIManager");
        _inventoryStore = _uiManager.GetComponent<InventoryStore>();
    }

    public void ToggleAutoEquip()
    {
        _inventoryStore.isAutoEquipEnabled = !_inventoryStore.isAutoEquipEnabled;
    }

    public void ToggleAutoSwitchLockTarget()
    {
        _lockOnController.isAutoSwitchEnabled = !_lockOnController.isAutoSwitchEnabled;
    }

    public void ForceControlScheme(int controlScheme)
    {
        switch (controlScheme)
        {
            case 0: // automatically detect control scheme
                _itemPickupHandler.forceControlScheme = false;
                break;
            case 1: // force control scheme to be Xbox
                _itemPickupHandler.forceControlScheme = true;
                _itemPickupHandler.currentControl = ItemPickupHandler.ControlScheme.Xbox;
                break;
            case 2: // force control scheme to be PlayStation
                _itemPickupHandler.forceControlScheme = true;
                    _itemPickupHandler.currentControl = ItemPickupHandler.ControlScheme.Playstation;
                break;
            case 3: // force control scheme to be Keyboard and Mouse
                _itemPickupHandler.forceControlScheme = true;
                _itemPickupHandler.currentControl = ItemPickupHandler.ControlScheme.Keyboard;
                break;
        }
    }
}
