using UnityEngine;

public class SettingManager : MonoBehaviour
{
    private GameObject _player;
    private GameObject _uiManager;
    private LockOnController _lockOnController;
    private InventoryStore _inventoryStore;

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        _lockOnController = _player.GetComponent<LockOnController>();
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
}
