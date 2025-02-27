using TMPro;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public int currencyHeld;
    [SerializeField] private TextMeshProUGUI currencyHeldText;

    private void Start()
    {
        UpdateCurrency(100);
    }

    public void UpdateCurrency(int amount)
    {
        if (currencyHeld + amount <= 0)
        {
            currencyHeld = 0;
        }
        else
        {
            currencyHeld += amount;   
        }
        
        currencyHeldText.text = currencyHeld.ToString();
    }
}
