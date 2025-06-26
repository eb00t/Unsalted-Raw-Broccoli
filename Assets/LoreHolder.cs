using UnityEngine;


public class LoreHolder : MonoBehaviour
{
    public LoreItemHandler loreItemHandler;

    public void OnLoreButtonPressed()
    {
        MenuHandler.Instance.LoreButtonPressed();
    }
}
