using Unity.VisualScripting;
using UnityEngine;

public class SettingManager : MonoBehaviour
{
    private InventoryStore _inventoryStore;
    [SerializeField] private DataHolder dataHolder;

    public void ToggleAutoEquip()
    {
        dataHolder.isAutoEquipEnabled = !dataHolder.isAutoEquipEnabled;
    }

    public void ToggleAutoSwitchLockTarget()
    {
        dataHolder.isAutoSwitchEnabled = !dataHolder.isAutoSwitchEnabled;
    }

    public void ForceControlScheme(int controlScheme)
    {
        switch (controlScheme)
        {
            case 0: // automatically detect control scheme
                dataHolder.forceControlScheme = false;
                break;
            case 1: // force control scheme to be Xbox
                dataHolder.forceControlScheme = true;
                dataHolder.currentControl = ControlsManager.ControlScheme.Xbox;
                break;
            case 2: // force control scheme to be PlayStation
                dataHolder.forceControlScheme = true;
                dataHolder.currentControl = ControlsManager.ControlScheme.Playstation;
                break;
            case 3: // force control scheme to be Keyboard and Mouse
                dataHolder.forceControlScheme = true;
                dataHolder.currentControl = ControlsManager.ControlScheme.Keyboard;
                break;
        }
    }
}
