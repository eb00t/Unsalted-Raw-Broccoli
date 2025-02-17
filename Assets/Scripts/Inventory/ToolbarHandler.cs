using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;

public class ToolbarHandler : MonoBehaviour
{
    public int slotNo;
    [SerializeField] private GameObject[] slots;
    private InventoryStore _inventoryStore;
    [SerializeField] private Consumable[] activeConsumables = new Consumable[3];
    private GameObject _player;
    private CharacterAttack _characterAttack;

    private void Start()
    {
        _inventoryStore = GetComponent<InventoryStore>();
        _player = GameObject.FindGameObjectWithTag("Player");
        _characterAttack = _player.GetComponentInChildren<CharacterAttack>();
    }

    private void AddToToolbar(Sprite newSprite, string txt, Consumable consumable)
    {
        Debug.Log(slotNo);
        slots[slotNo].GetComponentInChildren<TextMeshProUGUI>().text = ""; // set amount held
        activeConsumables[slotNo] = consumable;

        foreach (var s in slots[slotNo].GetComponentsInChildren<Image>())
        {
            if (s.name == "Image")
            {
                s.sprite = newSprite;
                foreach (var t in s.GetComponentsInChildren<TextMeshProUGUI>())
                {
                    if (t.name == "title")
                    {
                        t.text = txt;
                    }
                }

                s.enabled = true;
            }
        }
    }

    public void InvItemSelected(IndexHolder indexHolder)
    {
        var consumable = _inventoryStore.items[indexHolder.InventoryIndex].GetComponent<Consumable>();
        var s = consumable.uiIcon;
        var t = consumable.title;

        AddToToolbar(s, t, consumable);
    }

    public void SlotItemActivated(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (_player.GetComponent<CharacterMovement>().uiOpen) return;

        var dir = context.ReadValue<Vector2>();

        switch (dir.x, dir.y)
        {
            case (0, 1): // up (0)
                // use consumable in slot 0, and so on for each case
                CheckItemEffect(0);
                break;
            case (1, 0): // right (1)
                CheckItemEffect(1);
                break;
            case (0, -1): // down (2)
                CheckItemEffect(2);
                break;
            case (-1, 0): // left (3)
                CheckItemEffect(3);
                break;
        }
    }

    private void CheckItemEffect(int num)
    {
        var effect = activeConsumables[num].consumableEffect;

        switch (effect)
        {
            case ConsumableEffect.None:
                Debug.Log("Item has no effect assigned.");
                break;
            case ConsumableEffect.Heal:
                Debug.Log("Healing item used");
                if (_characterAttack.currentHealth + (_characterAttack.maxHealth / 2) >= _characterAttack.maxHealth)
                {
                    _characterAttack.currentHealth = _characterAttack.maxHealth;
                }
                else if (_characterAttack.currentHealth + (_characterAttack.maxHealth / 2) < _characterAttack.maxHealth)
                {
                    _characterAttack.currentHealth += _characterAttack.maxHealth / 2; 
                }
                
                _characterAttack.TakeDamagePlayer(0); // to update ui
                break;
        }
    }
}