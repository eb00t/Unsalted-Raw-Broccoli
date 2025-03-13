using TMPro;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    [SerializeField] private DataHolder dataHolder;
    [SerializeField] private TextMeshProUGUI currencyHeldText;

    public void UpdateCurrency(int amount)
    {
        if (dataHolder.currencyHeld + amount <= 0)
        {
            dataHolder.currencyHeld = 0;
        }
        else
        {
            dataHolder.currencyHeld += amount;   
        }
        
        currencyHeldText.text = dataHolder.currencyHeld.ToString();
    }
}
